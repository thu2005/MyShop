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
    const productData = [
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

    const products = [];
    for (const product of productData) {
      const created = await prisma.product.upsert({
        where: { sku: product.sku },
        update: {},
        create: product,
      });
      products.push(created);
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
      prisma.customer.upsert({
        where: { email: 'bob.wilson@example.com' },
        update: {},
        create: {
          name: 'Bob Wilson',
          email: 'bob.wilson@example.com',
          phone: '+1234567892',
          address: '789 Pine St, City, State 12345',
          isMember: true,
          memberSince: new Date('2024-06-15'),
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

    // 6. Seed Orders
    console.log('📦 Seeding orders...');
    const orders = await Promise.all([
      prisma.order.upsert({
        where: { orderNumber: 'ORD-2024-001' },
        update: {},
        create: {
          orderNumber: 'ORD-2024-001',
          customerId: customers[0].id,
          userId: users[2].id, // staff1
          status: 'COMPLETED',
          subtotal: new Prisma.Decimal(500),
          discountAmount: new Prisma.Decimal(50),
          taxAmount: new Prisma.Decimal(45),
          total: new Prisma.Decimal(495),
          completedAt: new Date('2024-12-01'),
          orderItems: {
            create: [
              {
                productId: products[0].id,
                quantity: 2,
                unitPrice: new Prisma.Decimal(150),
                subtotal: new Prisma.Decimal(300),
                discountAmount: new Prisma.Decimal(30),
                total: new Prisma.Decimal(270),
              },
              {
                productId: products[1].id,
                quantity: 1,
                unitPrice: new Prisma.Decimal(200),
                subtotal: new Prisma.Decimal(200),
                discountAmount: new Prisma.Decimal(20),
                total: new Prisma.Decimal(180),
              },
            ],
          },
        },
      }),
      prisma.order.upsert({
        where: { orderNumber: 'ORD-2024-002' },
        update: {},
        create: {
          orderNumber: 'ORD-2024-002',
          customerId: customers[1].id,
          userId: users[2].id,
          status: 'COMPLETED',
          subtotal: new Prisma.Decimal(1200),
          discountAmount: new Prisma.Decimal(0),
          taxAmount: new Prisma.Decimal(120),
          total: new Prisma.Decimal(1320),
          completedAt: new Date('2024-12-05'),
          orderItems: {
            create: [
              {
                productId: products[2].id,
                quantity: 3,
                unitPrice: new Prisma.Decimal(400),
                subtotal: new Prisma.Decimal(1200),
                discountAmount: new Prisma.Decimal(0),
                total: new Prisma.Decimal(1200),
              },
            ],
          },
        },
      }),
      prisma.order.upsert({
        where: { orderNumber: 'ORD-2024-003' },
        update: {},
        create: {
          orderNumber: 'ORD-2024-003',
          customerId: customers[2].id,
          userId: users[1].id, // manager1
          status: 'PENDING',
          subtotal: new Prisma.Decimal(300),
          discountAmount: new Prisma.Decimal(0),
          taxAmount: new Prisma.Decimal(30),
          total: new Prisma.Decimal(330),
          orderItems: {
            create: [
              {
                productId: products[3].id,
                quantity: 2,
                unitPrice: new Prisma.Decimal(150),
                subtotal: new Prisma.Decimal(300),
                discountAmount: new Prisma.Decimal(0),
                total: new Prisma.Decimal(300),
              },
            ],
          },
        },
      }),
    ]);
    console.log(`   ✅ Created ${orders.length} orders\n`);

    // 7. Seed License Keys for testing
    console.log('🔑 Seeding license keys...');
    const licenses = await Promise.all([
      prisma.appLicense.upsert({
        where: { licenseKey: 'MYSHOP-TEST-0001' },
        update: {},
        create: {
          licenseKey: 'MYSHOP-TEST-0001',
          activatedAt: new Date('2024-12-01'),
          expiresAt: new Date('2025-12-01'), // 1 year
          isActive: true,
        },
      }),
      prisma.appLicense.upsert({
        where: { licenseKey: 'MYSHOP-TRIAL-0001' },
        update: {},
        create: {
          licenseKey: 'MYSHOP-TRIAL-0001',
          activatedAt: new Date(),
          expiresAt: new Date(new Date().getTime() + 15 * 24 * 60 * 60 * 1000), // 15 days
          isActive: true,
        },
      }),
    ]);
    console.log(`   ✅ Created ${licenses.length} license keys\n`);

    console.log('✨ Database seeding completed successfully!\n');
    console.log('📊 Summary:');
    console.log(`   - Users: ${users.length}`);
    console.log(`   - Categories: ${categories.length}`);
    console.log(`   - Products: ${products.length}`);
    console.log(`   - Customers: ${customers.length}`);
    console.log(`   - Discounts: ${discounts.length}`);
    console.log(`   - Orders: ${orders.length}`);
    console.log(`   - License Keys: ${licenses.length}\n`);
    console.log('🔐 Available License Keys for Testing:');
    licenses.forEach((lic) => {
      console.log(`   - ${lic.licenseKey} (expires: ${lic.expiresAt})`);
    });
  } catch (error) {
    console.error('❌ Error seeding database:', error);
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

// Run the seeder
seed();
