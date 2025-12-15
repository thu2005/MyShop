/**
 * Admin Seeder Script
 *
 * This script creates a hardcoded admin user for the system.
 * Run with: pnpm seed:admin
 *
 * Default credentials:
 * - Username: admin
 * - Email: admin@myshop.com
 * - Password: Admin@123456
 * - Role: ADMIN
 */

import { PrismaClient } from '@prisma/client';
import { AuthUtils } from './auth';
import { env } from '../config/env';

const prisma = new PrismaClient();

async function seedAdmin() {
  try {
    console.log('🌱 Starting admin seeding...\n');

    // Admin credentials from environment or defaults
    const adminData = {
      username: env.ADMIN_USERNAME,
      email: env.ADMIN_EMAIL,
      password: env.ADMIN_PASSWORD,
      role: 'ADMIN' as const,
    };

    console.log('📋 Admin details:');
    console.log(`   Username: ${adminData.username}`);
    console.log(`   Email: ${adminData.email}`);
    console.log(`   Role: ${adminData.role}\n`);

    // Check if admin already exists
    const existingAdmin = await prisma.user.findFirst({
      where: {
        OR: [
          { username: adminData.username },
          { email: adminData.email },
        ],
      },
    });

    if (existingAdmin) {
      console.log('⚠️  Admin user already exists!');
      console.log(`   ID: ${existingAdmin.id}`);
      console.log(`   Username: ${existingAdmin.username}`);
      console.log(`   Email: ${existingAdmin.email}`);
      console.log(`   Role: ${existingAdmin.role}\n`);

      // Ask if user wants to update password
      console.log('💡 To update admin password, delete the existing user first or use a different username/email.\n');
      return;
    }

    // Hash password
    console.log('🔐 Hashing password...');
    const hashedPassword = await AuthUtils.hashPassword(adminData.password);

    // Create admin user
    console.log('👤 Creating admin user...');
    const admin = await prisma.user.create({
      data: {
        username: adminData.username,
        email: adminData.email,
        password: hashedPassword,
        role: adminData.role,
        isActive: true,
      },
    });

    console.log('\n✅ Admin user created successfully!\n');
    console.log('📝 Login credentials:');
    console.log(`   Username: ${admin.username}`);
    console.log(`   Email: ${admin.email}`);
    console.log(`   Password: ${adminData.password}`);
    console.log(`   Role: ${admin.role}`);
    console.log(`\n🔒 Please change the password after first login!\n`);

  } catch (error) {
    console.error('❌ Error seeding admin:', error);
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

// Run the seeder
seedAdmin();
