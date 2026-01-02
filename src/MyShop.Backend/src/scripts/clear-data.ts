import * as readline from 'readline';
import prisma from '../config/database';

async function askConfirmation(message: string): Promise<boolean> {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
  });

  return new Promise((resolve) => {
    rl.question(message, (answer) => {
      rl.close();
      resolve(answer.toLowerCase() === 'y' || answer.toLowerCase() === 'yes');
    });
  });
}

async function clearAllData(force: boolean = false) {
  try {
    console.log('\n🗑️  Database Clear Script\n');
    console.log('⚠️  WARNING: This will DELETE ALL DATA from the database!');
    console.log('⚠️  The database schema will be preserved.\n');

    // Get record counts
    console.log('📊 Current Data Statistics:');
    const counts = {
      users: await prisma.user.count(),
      categories: await prisma.category.count(),
      products: await prisma.product.count(),
      productImages: await prisma.productImage.count(),
      customers: await prisma.customer.count(),
      discounts: await prisma.discount.count(),
      orders: await prisma.order.count(),
      orderItems: await prisma.orderItem.count(),
      commissions: await prisma.commission.count(),
      salesTargets: await prisma.salesTarget.count(),
      appLicenses: await prisma.appLicense.count(),
    };

    console.log(`   Users: ${counts.users}`);
    console.log(`   Categories: ${counts.categories}`);
    console.log(`   Products: ${counts.products}`);
    console.log(`   Product Images: ${counts.productImages}`);
    console.log(`   Customers: ${counts.customers}`);
    console.log(`   Discounts: ${counts.discounts}`);
    console.log(`   Orders: ${counts.orders}`);
    console.log(`   Order Items: ${counts.orderItems}`);
    console.log(`   Commissions: ${counts.commissions}`);
    console.log(`   Sales Targets: ${counts.salesTargets}`);
    console.log(`   App Licenses: ${counts.appLicenses}\n`);

    const totalRecords = Object.values(counts).reduce((sum, count) => sum + count, 0);

    if (totalRecords === 0) {
      console.log('✅ Database is already empty!\n');
      return;
    }

    console.log(`📈 Total Records: ${totalRecords}\n`);

    // Confirmation
    if (!force) {
      const confirmed = await askConfirmation('❓ Are you absolutely sure you want to DELETE ALL DATA? (y/N): ');

      if (!confirmed) {
        console.log('\n❌ Operation cancelled by user.\n');
        process.exit(0);
      }

      // Double confirmation
      const doubleConfirmed = await askConfirmation('❓ Last chance! Type "yes" to confirm: ');

      if (!doubleConfirmed) {
        console.log('\n❌ Operation cancelled by user.\n');
        process.exit(0);
      }
    }

    console.log('\n🔥 Starting data deletion...\n');

    // Delete in correct order (respecting foreign key constraints)
    // Bottom-up approach: delete children first

    console.log('⏳ Deleting order items...');
    const deletedOrderItems = await prisma.orderItem.deleteMany();
    console.log(`   ✅ Deleted ${deletedOrderItems.count} order items`);

    console.log('⏳ Deleting orders...');
    const deletedOrders = await prisma.order.deleteMany();
    console.log(`   ✅ Deleted ${deletedOrders.count} orders`);

    console.log('⏳ Deleting product images...');
    const deletedImages = await prisma.productImage.deleteMany();
    console.log(`   ✅ Deleted ${deletedImages.count} product images`);

    console.log('⏳ Deleting products...');
    const deletedProducts = await prisma.product.deleteMany();
    console.log(`   ✅ Deleted ${deletedProducts.count} products`);

    console.log('⏳ Deleting categories...');
    const deletedCategories = await prisma.category.deleteMany();
    console.log(`   ✅ Deleted ${deletedCategories.count} categories`);

    console.log('⏳ Deleting customers...');
    const deletedCustomers = await prisma.customer.deleteMany();
    console.log(`   ✅ Deleted ${deletedCustomers.count} customers`);

    console.log('⏳ Deleting discounts...');
    const deletedDiscounts = await prisma.discount.deleteMany();
    console.log(`   ✅ Deleted ${deletedDiscounts.count} discounts`);

    console.log('⏳ Deleting commissions...');
    const deletedCommissions = await prisma.commission.deleteMany();
    console.log(`   ✅ Deleted ${deletedCommissions.count} commissions`);

    console.log('⏳ Deleting sales targets...');
    const deletedTargets = await prisma.salesTarget.deleteMany();
    console.log(`   ✅ Deleted ${deletedTargets.count} sales targets`);

    console.log('⏳ Deleting app licenses...');
    const deletedLicenses = await prisma.appLicense.deleteMany();
    console.log(`   ✅ Deleted ${deletedLicenses.count} app licenses`);

    console.log('⏳ Deleting users...');
    const deletedUsers = await prisma.user.deleteMany();
    console.log(`   ✅ Deleted ${deletedUsers.count} users`);

    // Reset sequences (auto-increment IDs)
    console.log('\n🔄 Resetting auto-increment sequences...');

    await prisma.$executeRawUnsafe('ALTER SEQUENCE users_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE categories_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE products_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE product_images_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE customers_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE discounts_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE orders_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE order_items_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE commissions_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE sales_targets_id_seq RESTART WITH 1');
    await prisma.$executeRawUnsafe('ALTER SEQUENCE app_licenses_id_seq RESTART WITH 1');

    console.log('   ✅ All sequences reset to 1');

    console.log('\n✅ Database cleared successfully!\n');
    console.log('📊 Final Summary:');
    console.log(`   Total records deleted: ${totalRecords}`);
    console.log(`   Database schema: PRESERVED`);
    console.log(`   Auto-increment IDs: RESET`);
    console.log(`   Timestamp: ${new Date().toLocaleString()}\n`);

    console.log('💡 Tip: You can now run "npm run seed" to populate with sample data.\n');

  } catch (error) {
    console.error('\n❌ Error clearing data:', error);
    throw error;
  } finally {
    await prisma.$disconnect();
  }
}

// CLI Usage
const args = process.argv.slice(2);
const forceArg = args.includes('--force') || args.includes('-f');

if (args.includes('--help') || args.includes('-h')) {
  console.log(`
📚 Database Clear Script

⚠️  WARNING: This script DELETES ALL DATA from the database!

Usage:
  npm run clear-data              # Interactive mode with confirmations
  npm run clear-data -- --force   # Skip confirmations (use with caution!)

Options:
  --force, -f        Skip confirmation prompts (dangerous!)
  --help, -h         Show this help message

What it does:
  ✓ Deletes all records from all tables
  ✓ Preserves database schema (tables, columns, constraints)
  ✓ Resets auto-increment sequences to 1
  ✗ Does NOT drop tables
  ✗ Does NOT delete migrations

Use cases:
  - Demo/testing environments
  - Reset to clean state
  - Before restoring from backup
  - Development testing

Safety:
  - Always backup before running this script!
  - Use npm run backup before clearing data
  - Never use --force in production

Example workflow:
  1. npm run backup              # Create backup first!
  2. npm run clear-data          # Clear all data
  3. npm run seed                # Repopulate with sample data
  4. npm run restore -- --file=backup.dump  # Or restore from backup

⚠️  USE WITH EXTREME CAUTION! ⚠️
  `);
  process.exit(0);
}

clearAllData(forceArg)
  .then(() => {
    console.log('✨ Done!\n');
    process.exit(0);
  })
  .catch((error) => {
    console.error('\n💥 Error:', error.message);
    process.exit(1);
  });
