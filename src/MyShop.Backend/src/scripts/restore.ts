import { PrismaClient } from '@prisma/client';
import * as fs from 'fs';
import * as path from 'path';

const prisma = new PrismaClient();

function getLatestBackupFile(): string {
  const backupDir = path.join(__dirname, '../../backups');

  if (!fs.existsSync(backupDir)) {
    throw new Error(`Backup directory not found: ${backupDir}`);
  }

  const files = fs.readdirSync(backupDir)
    .filter(file => file.startsWith('myshop_backup_') && file.endsWith('.json'))
    .sort()
    .reverse();

  if (files.length === 0) {
    throw new Error('No backup files found in the backups directory');
  }

  return files[0];
}

async function restoreBackup(backupFile?: string) {
  try {
    console.log('Starting database restore...\n');

    const backupDir = path.join(__dirname, '../../backups');
    const fileName = backupFile || getLatestBackupFile();

    const backupPath = path.isAbsolute(fileName)
      ? fileName
      : path.join(backupDir, fileName);

    if (!fs.existsSync(backupPath)) {
      throw new Error(`Backup file not found: ${backupPath}`);
    }

    console.log(`Reading backup file: ${backupPath}\n`);

    const backupData = fs.readFileSync(backupPath, 'utf8');
    const backup = JSON.parse(backupData);

    console.log('Backup Information:');
    console.log(`   Created: ${backup.metadata.timestamp}`);
    console.log(`   Version: ${backup.metadata.version}`);
    console.log(`   Database: ${backup.metadata.database}\n`);

    console.log('Data to restore:');
    Object.entries(backup.statistics).forEach(([key, value]) => {
      if (key !== 'total') {
        console.log(`   ${key}: ${value} records`);
      }
    });
    console.log(`   ─────────────────`);
    console.log(`   Total: ${backup.statistics.total} records\n`);

    console.log('WARNING: This will DELETE all existing data and restore from backup!\n');
    console.log('Starting restore process...\n');

    console.log('Clearing existing data...');
    await prisma.commission.deleteMany();
    await prisma.salesTarget.deleteMany();
    await prisma.orderItem.deleteMany();
    await prisma.order.deleteMany();
    await prisma.productImage.deleteMany();
    await prisma.product.deleteMany();
    await prisma.category.deleteMany();
    await prisma.customer.deleteMany();
    await prisma.discount.deleteMany();
    await prisma.appLicense.deleteMany();
    await prisma.user.deleteMany();
    console.log('   Existing data cleared\n');

    console.log('Restoring data...');

    if (backup.data.users?.length > 0) {
      await prisma.user.createMany({ data: backup.data.users });
      console.log(`   Users: ${backup.data.users.length} records`);
    }

    if (backup.data.categories?.length > 0) {
      await prisma.category.createMany({ data: backup.data.categories });
      console.log(`   Categories: ${backup.data.categories.length} records`);
    }

    if (backup.data.products?.length > 0) {
      await prisma.product.createMany({ data: backup.data.products });
      console.log(`   Products: ${backup.data.products.length} records`);
    }

    if (backup.data.productImages?.length > 0) {
      await prisma.productImage.createMany({ data: backup.data.productImages });
      console.log(`   Product Images: ${backup.data.productImages.length} records`);
    }

    if (backup.data.customers?.length > 0) {
      await prisma.customer.createMany({ data: backup.data.customers });
      console.log(`   Customers: ${backup.data.customers.length} records`);
    }

    if (backup.data.discounts?.length > 0) {
      await prisma.discount.createMany({ data: backup.data.discounts });
      console.log(`   Discounts: ${backup.data.discounts.length} records`);
    }

    if (backup.data.orders?.length > 0) {
      await prisma.order.createMany({ data: backup.data.orders });
      console.log(`   Orders: ${backup.data.orders.length} records`);
    }

    if (backup.data.orderItems?.length > 0) {
      await prisma.orderItem.createMany({ data: backup.data.orderItems });
      console.log(`   Order Items: ${backup.data.orderItems.length} records`);
    }

    if (backup.data.appLicenses?.length > 0) {
      await prisma.appLicense.createMany({ data: backup.data.appLicenses });
      console.log(`   App Licenses: ${backup.data.appLicenses.length} records`);
    }

    if (backup.data.commissions?.length > 0) {
      await prisma.commission.createMany({ data: backup.data.commissions });
      console.log(`   Commissions: ${backup.data.commissions.length} records`);
    }

    if (backup.data.salesTargets?.length > 0) {
      await prisma.salesTarget.createMany({ data: backup.data.salesTargets });
      console.log(`   Sales Targets: ${backup.data.salesTargets.length} records`);
    }

    console.log('\nRestore completed successfully!\n');

    const [
      usersCount,
      categoriesCount,
      productsCount,
      productImagesCount,
      customersCount,
      discountsCount,
      ordersCount,
      orderItemsCount,
      appLicensesCount,
      commissionsCount,
      salesTargetsCount,
    ] = await Promise.all([
      prisma.user.count(),
      prisma.category.count(),
      prisma.product.count(),
      prisma.productImage.count(),
      prisma.customer.count(),
      prisma.discount.count(),
      prisma.order.count(),
      prisma.orderItem.count(),
      prisma.appLicense.count(),
      prisma.commission.count(),
      prisma.salesTarget.count(),
    ]);

    console.log('Verification:');
    console.log(`   Users: ${usersCount}`);
    console.log(`   Categories: ${categoriesCount}`);
    console.log(`   Products: ${productsCount}`);
    console.log(`   Product Images: ${productImagesCount}`);
    console.log(`   Customers: ${customersCount}`);
    console.log(`   Discounts: ${discountsCount}`);
    console.log(`   Orders: ${ordersCount}`);
    console.log(`   Order Items: ${orderItemsCount}`);
    console.log(`   App Licenses: ${appLicensesCount}`);
    console.log(`   Commissions: ${commissionsCount}`);
    console.log(`   Sales Targets: ${salesTargetsCount}`);

    const totalCount =
      usersCount +
      categoriesCount +
      productsCount +
      productImagesCount +
      customersCount +
      discountsCount +
      ordersCount +
      orderItemsCount +
      appLicensesCount +
      commissionsCount +
      salesTargetsCount;

    console.log(`   ─────────────────`);
    console.log(`   Total: ${totalCount} records\n`);

    return {
      success: true,
      recordsRestored: totalCount,
    };
  } catch (error) {
    console.error('Restore failed:', error);
    throw error;
  } finally {
    await prisma.$disconnect();
  }
}

const args = process.argv.slice(2);

if (args.includes('--help') || args.includes('-h')) {
  console.log(`
Database Restore Script

Usage:
  npm run db:restore                # Restore from latest backup
  npm run db:restore <backup-file>  # Restore from specific backup

Arguments:
  <backup-file>    Name of backup file in backups/ directory
                   or absolute path to backup file
                   (optional - defaults to latest backup)

Options:
  --help, -h       Show this help message

Examples:
  npm run db:restore
  npm run db:restore myshop_backup_2025-12-30_21-00-00.json
  npm run db:restore /path/to/backup.json

WARNING: This will DELETE all existing data!
  `);
  process.exit(0);
}

const backupFile = args[0];

restoreBackup(backupFile)
  .then(() => {
    console.log('Done!\n');
    process.exit(0);
  })
  .catch((error) => {
    console.error('\nError:', error.message);
    process.exit(1);
  });
