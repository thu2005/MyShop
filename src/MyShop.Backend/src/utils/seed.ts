/**
 * Database Seeder Script
 *
 * This script seeds the database with sample data for development/testing.
 * Run with: pnpm seed
 */

import { PrismaClient, Prisma } from '@prisma/client';
import { AuthUtils } from './auth';

const prisma = new PrismaClient();

async function seed() {
  try {
    console.log('🌱 Starting database seeding...\n');

    // 1. Seed Users
    console.log('👥 Seeding users...');
    const hashedPassword = await AuthUtils.hashPassword('Password@123');

    const users = await Promise.all([
      prisma.user.upsert({
        where: { username: 'admin' },
        update: {},
        create: {
          username: 'admin',
          email: 'admin@myshop.com',
          password: await AuthUtils.hashPassword('Admin@123456'),
          role: 'ADMIN',
        },
      }),
      prisma.user.upsert({
        where: { username: 'manager1' },
        update: {},
        create: {
          username: 'manager1',
          email: 'manager@myshop.com',
          password: hashedPassword,
          role: 'MANAGER',
        },
      }),
      prisma.user.upsert({
        where: { username: 'staff1' },
        update: {},
        create: {
          username: 'staff1',
          email: 'staff1@myshop.com',
          password: hashedPassword,
          role: 'STAFF',
        },
      }),
    ]);
    console.log(`   ✅ Created ${users.length} users\n`);

    // 2. Seed Categories
    console.log('📁 Seeding categories...');
    const categories = await Promise.all([
      prisma.category.upsert({
        where: { name: 'Electronics' },
        update: {},
        create: {
          name: 'Electronics',
          description: 'Electronic devices and accessories',
        },
      }),
      prisma.category.upsert({
        where: { name: 'Clothing' },
        update: {},
        create: {
          name: 'Clothing',
          description: 'Apparel and fashion items',
        },
      }),
      prisma.category.upsert({
        where: { name: 'Home & Garden' },
        update: {},
        create: {
          name: 'Home & Garden',
          description: 'Home improvement and garden supplies',
        },
      }),
      prisma.category.upsert({
        where: { name: 'Books' },
        update: {},
        create: {
          name: 'Books',
          description: 'Books and reading materials',
        },
      }),
      prisma.category.upsert({
        where: { name: 'Sports' },
        update: {},
        create: {
          name: 'Sports',
          description: 'Sports equipment and accessories',
        },
      }),
    ]);
    console.log(`   ✅ Created ${categories.length} categories\n`);

    // 3. Seed Products
    console.log('📦 Seeding products...');
    const products = [
      {
        name: 'Laptop HP Pavilion',
        sku: 'ELEC-001',
        barcode: '1234567890123',
        price: new Prisma.Decimal(799.99),
        costPrice: new Prisma.Decimal(650),
        stock: 15,
        minStock: 5,
        categoryId: categories[0].id,
        description: 'High-performance laptop for work and play',
      },
      {
        name: 'Wireless Mouse',
        sku: 'ELEC-002',
        barcode: '1234567890124',
        price: new Prisma.Decimal(29.99),
        costPrice: new Prisma.Decimal(15),
        stock: 50,
        minStock: 10,
        categoryId: categories[0].id,
        description: 'Ergonomic wireless mouse',
      },
      {
        name: 'Cotton T-Shirt',
        sku: 'CLTH-001',
        barcode: '1234567890125',
        price: new Prisma.Decimal(19.99),
        costPrice: new Prisma.Decimal(8),
        stock: 100,
        minStock: 20,
        categoryId: categories[1].id,
        description: 'Comfortable cotton t-shirt',
      },
      {
        name: 'Jeans',
        sku: 'CLTH-002',
        barcode: '1234567890126',
        price: new Prisma.Decimal(49.99),
        costPrice: new Prisma.Decimal(25),
        stock: 75,
        minStock: 15,
        categoryId: categories[1].id,
        description: 'Classic blue jeans',
      },
      {
        name: 'Garden Hose',
        sku: 'HOME-001',
        barcode: '1234567890127',
        price: new Prisma.Decimal(34.99),
        costPrice: new Prisma.Decimal(18),
        stock: 30,
        minStock: 10,
        categoryId: categories[2].id,
        description: '50ft expandable garden hose',
      },
    ];

    for (const product of products) {
      await prisma.product.upsert({
        where: { sku: product.sku },
        update: {},
        create: product,
      });
    }
    console.log(`   ✅ Created ${products.length} products\n`);

    // 4. Seed Customers
    console.log('👤 Seeding customers...');
    const customers = await Promise.all([
      prisma.customer.upsert({
        where: { email: 'john.doe@example.com' },
        update: {},
        create: {
          name: 'John Doe',
          email: 'john.doe@example.com',
          phone: '+1234567890',
          address: '123 Main St, City, State 12345',
          isMember: true,
          memberSince: new Date('2024-01-01'),
        },
      }),
      prisma.customer.upsert({
        where: { email: 'jane.smith@example.com' },
        update: {},
        create: {
          name: 'Jane Smith',
          email: 'jane.smith@example.com',
          phone: '+1234567891',
          address: '456 Oak Ave, City, State 12345',
          isMember: false,
        },
      }),
    ]);
    console.log(`   ✅ Created ${customers.length} customers\n`);

    // 5. Seed Discounts
    console.log('🎟️  Seeding discounts...');
    const discounts = await Promise.all([
      prisma.discount.upsert({
        where: { code: 'SAVE10' },
        update: {},
        create: {
          code: 'SAVE10',
          name: '10% Off',
          description: 'Get 10% off your purchase',
          type: 'PERCENTAGE',
          value: new Prisma.Decimal(10),
          minPurchase: new Prisma.Decimal(50),
          maxDiscount: new Prisma.Decimal(100),
          applicableToAll: true,
        },
      }),
      prisma.discount.upsert({
        where: { code: 'MEMBER20' },
        update: {},
        create: {
          code: 'MEMBER20',
          name: 'Member Discount',
          description: '20% off for members',
          type: 'MEMBER_DISCOUNT',
          value: new Prisma.Decimal(20),
          memberOnly: true,
          applicableToAll: true,
        },
      }),
    ]);
    console.log(`   ✅ Created ${discounts.length} discounts\n`);

    console.log('✨ Database seeding completed successfully!\n');
    console.log('📊 Summary:');
    console.log(`   - Users: ${users.length}`);
    console.log(`   - Categories: ${categories.length}`);
    console.log(`   - Products: ${products.length}`);
    console.log(`   - Customers: ${customers.length}`);
    console.log(`   - Discounts: ${discounts.length}\n`);
  } catch (error) {
    console.error('❌ Error seeding database:', error);
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

// Run the seeder
seed();
