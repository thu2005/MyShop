import { Context } from '../../types/context';
import { randomBytes } from 'crypto';

export interface ActivateLicenseInput {
    licenseKey: string;
}

// Generate license key: MYSHOP-XXXX-XXXX-XXXX
function generateLicenseKey(): string {
    const part1 = randomBytes(2).toString('hex').toUpperCase();
    const part2 = randomBytes(2).toString('hex').toUpperCase();
    const part3 = randomBytes(2).toString('hex').toUpperCase();
    return `MYSHOP-${part1}-${part2}-${part3}`;
}

export const appLicenseResolvers = {
    Query: {
        // Validate license key
        validateLicense: async (
            _: any,
            { licenseKey }: { licenseKey: string },
            { prisma }: Context
        ) => {
            const license = await prisma.appLicense.findUnique({
                where: { licenseKey },
            });

            if (!license) {
                throw new Error('Invalid license key');
            }

            return license;
        },

        // Check current trial/license status (for app startup)
        checkTrialStatus: async (_: any, __: any, { prisma }: Context) => {
            // Get the most recent active license (trial or full)
            const license = await prisma.appLicense.findFirst({
                where: { isActive: true },
                orderBy: { activatedAt: 'desc' },
            });

            return license;

            return license;
        },
    },

    Mutation: {
        // Activate 15-day trial (only once per owner)
        activateTrial: async (
            _: any,
            _input: any,
            { prisma, user }: Context
        ) => {
            if (!user || user.role !== 'ADMIN') {
                throw new Error('Only admin can activate trial');
            }

            // Check if trial/license already exists
            const existing = await prisma.appLicense.findFirst({
                where: { isActive: true },
            });

            if (existing) {
                throw new Error('Trial already activated. Use license key to extend.');
            }

            // Create trial license (15 days from now)
            const now = new Date();
            const expiresAt = new Date(now.getTime() + 15 * 24 * 60 * 60 * 1000); // 15 days

            const license = await prisma.appLicense.create({
                data: {
                    licenseKey: generateLicenseKey(),
                    activatedAt: now,
                    expiresAt,
                    isActive: true,
                },
            });

            return license;
        },

        // Activate full license with key (extend from trial)
        activateLicense: async (
            _: any,
            { input }: { input: ActivateLicenseInput },
            { prisma, user }: Context
        ) => {
            if (!user || user.role !== 'ADMIN') {
                throw new Error('Only admin can activate license');
            }

            // Find license by key
            const license = await prisma.appLicense.findUnique({
                where: { licenseKey: input.licenseKey },
            });

            if (!license) {
                throw new Error('Invalid license key');
            }
            // Update license and extend expiry (1 year full version)
            const now = new Date();
            const expiresAt = new Date(now.getTime() + 365 * 24 * 60 * 60 * 1000); // 1 year

            const updatedLicense = await prisma.appLicense.update({
                where: { licenseKey: input.licenseKey },
                data: {
                    expiresAt,
                    isActive: true,
                },
            });

            return updatedLicense;
        },

        // Deactivate license
        deactivateLicense: async (
            _: any,
            { licenseKey }: { licenseKey: string },
            { prisma, user }: Context
        ) => {
            if (!user || user.role !== 'ADMIN') {
                throw new Error('Only admin can deactivate license');
            }

            await prisma.appLicense.update({
                where: { licenseKey },
                data: { isActive: false },
            });

            return true;
        },
    },

    AppLicense: {
        // Check if license is expired
        isExpired: (parent: any) => {
            return new Date() > new Date(parent.expiresAt);
        },

        // Calculate days remaining
        daysRemaining: (parent: any) => {
            const now = new Date();
            const expires = new Date(parent.expiresAt);
            const diff = expires.getTime() - now.getTime();
            const days = Math.ceil(diff / (1000 * 60 * 60 * 24));
            return days > 0 ? days : 0;
        },
    },
};
