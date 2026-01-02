import { PrismaClient } from '@prisma/client';
import * as fs from 'fs';
import * as path from 'path';

const prisma = new PrismaClient();

interface BackupOptions {
  format?: 'json' | 'sql';
}

async function createBackup(options: BackupOptions = {}) {
  const { format = 'json' } = options;

  try {
    console.log('Starting database backup...\n');

    const backupDir = path.join(__dirname, '../../backups');
    if (!fs.existsSync(backupDir)) {
      fs.mkdirSync(backupDir, { recursive: true });
      console.log(`Created backup directory: ${backupDir}\n`);
    }

    const timestamp =
      new Date().toISOString().replace(/[:.]/g, '-').split('T')[0] +
      '_' +
      new Date().toTimeString().split(' ')[0].replace(/:/g, '-');
    const extension = format === 'json' ? 'json' : 'sql';
    const filename = `myshop_backup_${timestamp}.${extension}`;
    const backupPath = path.join(backupDir, filename);

    console.log(`Backup format: ${format.toUpperCase()}`);
    console.log(`Filename: ${filename}`);
    console.log(`Path: ${backupPath}\n`);

    console.log('Exporting data from database...\n');

    const [
      users,
      categories,
      products,
      productImages,
      customers,
      discounts,
      orders,
      orderItems,
      appLicenses,
      commissions,
      salesTargets,
    ] = await Promise.all([
      prisma.user.findMany(),
      prisma.category.findMany(),
      prisma.product.findMany(),
      prisma.productImage.findMany(),
      prisma.customer.findMany(),
      prisma.discount.findMany(),
      prisma.order.findMany(),
      prisma.orderItem.findMany(),
      prisma.appLicense.findMany(),
      prisma.commission.findMany(),
      prisma.salesTarget.findMany(),
    ]);

    const backup = {
      metadata: {
        timestamp: new Date().toISOString(),
        version: '1.0',
        database: 'myshop',
      },
      data: {
        users,
        categories,
        products,
        productImages,
        customers,
        discounts,
        orders,
        orderItems,
        appLicenses,
        commissions,
        salesTargets,
      },
      statistics: {
        users: users.length,
        categories: categories.length,
        products: products.length,
        productImages: productImages.length,
        customers: customers.length,
        discounts: discounts.length,
        orders: orders.length,
        orderItems: orderItems.length,
        appLicenses: appLicenses.length,
        commissions: commissions.length,
        salesTargets: salesTargets.length,
        total:
          users.length +
          categories.length +
          products.length +
          productImages.length +
          customers.length +
          discounts.length +
          orders.length +
          orderItems.length +
          appLicenses.length +
          commissions.length +
          salesTargets.length,
      },
    };

    if (format === 'json') {
      const jsonData = JSON.stringify(backup, null, 2);
      fs.writeFileSync(backupPath, jsonData, 'utf8');
    } else {
      const sqlStatements: string[] = [];

      sqlStatements.push('-- MyShop Database Backup');
      sqlStatements.push(`-- Created: ${new Date().toISOString()}`);
      sqlStatements.push('-- Format: SQL INSERT statements\n');
      sqlStatements.push('BEGIN;\n');

      const escapeSqlValue = (value: any): string => {
        if (value === null || value === undefined) return 'NULL';
        if (typeof value === 'number') return value.toString();
        if (typeof value === 'boolean') return value ? 'TRUE' : 'FALSE';
        if (value instanceof Date) return `'${value.toISOString()}'`;
        if (typeof value === 'string') return `'${value.replace(/'/g, "''")}'`;
        if (typeof value === 'object') return `'${JSON.stringify(value).replace(/'/g, "''")}'`;
        return `'${String(value).replace(/'/g, "''")}'`;
      };

      const generateInserts = (tableName: string, records: any[]) => {
        if (records.length === 0) return;

        sqlStatements.push(`-- Table: ${tableName}`);
        sqlStatements.push(`TRUNCATE TABLE "${tableName}" RESTART IDENTITY CASCADE;`);

        records.forEach((record) => {
          const columns = Object.keys(record);
          const values = columns.map((col) => escapeSqlValue(record[col]));
          sqlStatements.push(
            `INSERT INTO "${tableName}" (${columns.map((c) => `"${c}"`).join(', ')}) VALUES (${values.join(', ')});`
          );
        });
        sqlStatements.push('');
      };

      generateInserts('users', users);
      generateInserts('categories', categories);
      generateInserts('products', products);
      generateInserts('product_images', productImages);
      generateInserts('customers', customers);
      generateInserts('discounts', discounts);
      generateInserts('orders', orders);
      generateInserts('order_items', orderItems);
      generateInserts('app_licenses', appLicenses);
      generateInserts('commissions', commissions);
      generateInserts('sales_targets', salesTargets);

      sqlStatements.push('COMMIT;');

      fs.writeFileSync(backupPath, sqlStatements.join('\n'), 'utf8');
    }

    const stats = fs.statSync(backupPath);
    const fileSizeMB = (stats.size / (1024 * 1024)).toFixed(2);

    console.log('Backup completed successfully!\n');
    console.log('Backup Details:');
    console.log(`   File: ${filename}`);
    console.log(`   Size: ${fileSizeMB} MB`);
    console.log(`   Path: ${backupPath}`);
    console.log(`   Format: ${format.toUpperCase()}`);
    console.log(`   Created: ${new Date().toLocaleString()}\n`);

    console.log('Data Statistics:');
    Object.entries(backup.statistics).forEach(([key, value]) => {
      if (key !== 'total') {
        console.log(`   ${key}: ${value} records`);
      }
    });
    console.log(`   ─────────────────`);
    console.log(`   Total: ${backup.statistics.total} records\n`);

    return {
      success: true,
      filename,
      path: backupPath,
      size: fileSizeMB,
      format,
      statistics: backup.statistics,
    };
  } catch (error) {
    console.error('Backup failed:', error);
    throw error;
  } finally {
    await prisma.$disconnect();
  }
}

const args = process.argv.slice(2);
const formatArg = args.find((arg) => arg.startsWith('--format='))?.split('=')[1] as
  | 'json'
  | 'sql'
  | undefined;

if (args.includes('--help') || args.includes('-h')) {
  console.log(`
Database Backup Script

Usage:
  npm run db:backup              # JSON format (default)
  npm run db:backup:sql          # SQL format

Options:
  --format=FORMAT    Output format: json or sql (default: json)
  --help, -h         Show this help message

Examples:
  npm run db:backup
  npm run db:backup -- --format=sql

Output:
  Backups are saved to: src/MyShop.Backend/backups/

Formats:
  - json: Full data export in JSON format
  - sql:  SQL INSERT statements
  `);
  process.exit(0);
}

createBackup({ format: formatArg })
  .then(() => {
    console.log('Done!\n');
    process.exit(0);
  })
  .catch((error) => {
    console.error('\nError:', error.message);
    process.exit(1);
  });
