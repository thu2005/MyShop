import { PrismaClient, Prisma, Product, Customer, Category, Discount } from '@prisma/client';
import { AuthUtils } from './auth';

const prisma = new PrismaClient();

async function seed() {
  try {
    console.log('Starting database seeding...');

    // 0. Clean up existing data (Child tables first to avoid FK errors)
    console.log('Cleaning up previous data...');
    await prisma.orderItem.deleteMany();
    await prisma.order.deleteMany();
    await prisma.product.deleteMany();
    await prisma.category.deleteMany();
    await prisma.discount.deleteMany();
    await prisma.customer.deleteMany();
    await prisma.user.deleteMany();
    await prisma.appLicense.deleteMany();
    console.log('Database cleaned.');

    // 1. Users
    console.log('Seeding users...');
    const hashedPassword = await AuthUtils.hashPassword('Password@123');

    const users = await Promise.all([
      prisma.user.create({
        data: { username: 'admin', email: 'admin@myshop.com', password: await AuthUtils.hashPassword('Admin@123456'), role: 'ADMIN' },
      }),
      prisma.user.create({
        data: { username: 'manager1', email: 'manager@myshop.com', password: hashedPassword, role: 'MANAGER' },
      }),
      prisma.user.create({
        data: { username: 'staff1', email: 'staff1@myshop.com', password: hashedPassword, role: 'STAFF' },
      }),
    ]);
    console.log(`Created ${users.length} users`);

    // 2. Categories
    console.log('Seeding categories...');
    const categoryData = [
      { name: 'iPhone', description: 'Apple Smartphones' },
      { name: 'iPad', description: 'Apple Tablets' },
      { name: 'Laptop', description: 'Portable computers' },
      { name: 'Tablet', description: 'Android and other tablets' },
      { name: 'PC', description: 'Desktop computers' },
      { name: 'TV', description: 'Smart Televisions' },
    ];

    const categories: Category[] = [];
    for (const cat of categoryData) {
      const c = await prisma.category.create({ data: cat });
      categories.push(c);
    }
    console.log(`Created ${categories.length} categories`);

    // 3. Discounts
    console.log('Seeding discounts...');
    const discountData = [
      { code: 'WELCOME2025', name: 'First Time User', description: 'Welcome gift for new users', type: 'FIXED_AMOUNT', value: 10, applicableToAll: true },
      { code: 'FREESHIP', name: 'Free Shipping', description: 'Free shipping on all orders', type: 'FIXED_AMOUNT', value: 5, applicableToAll: true },
      { code: 'MERRYXMAS', name: 'Merry Christmas', description: 'Holiday season special', type: 'PERCENTAGE', value: 20, applicableToAll: true },
      { code: 'FLASH50', name: 'Flash Sale', description: 'Limited time offer', type: 'PERCENTAGE', value: 50, applicableToAll: false },
      { code: 'VIPMEMBER', name: 'VIP Discount', description: 'Exclusive for members', type: 'PERCENTAGE', value: 15, applicableToAll: true },
    ];

    const discounts: Discount[] = [];
    for (const d of discountData) {
      const disc = await prisma.discount.create({
        data: {
          code: d.code,
          name: d.name,
          description: d.description,
          type: d.type as any,
          value: new Prisma.Decimal(d.value),
          applicableToAll: d.applicableToAll,
        },
      });
      discounts.push(disc);
    }

    // 4. Products
    console.log('Seeding products...');
    
    const iPhones = [
      { name: 'iPhone 15 Pro Max 256GB', price: 1199, cost: 900, sku: 'IPH-15PM-256', description: 'Titanium design, A17 Pro chip, 48MP Main camera.' },
      { name: 'iPhone 15 Pro Max 512GB', price: 1399, cost: 1000, sku: 'IPH-15PM-512', description: 'Titanium design, A17 Pro chip, massive 512GB storage.' },
      { name: 'iPhone 15 Pro Max 1TB', price: 1599, cost: 1200, sku: 'IPH-15PM-1TB', description: 'Titanium design, A17 Pro chip, ultimate 1TB storage.' },
      { name: 'iPhone 15 Pro 128GB', price: 999, cost: 750, sku: 'IPH-15P-128', description: '6.1-inch Super Retina XDR display, ProMotion technology.' },
      { name: 'iPhone 15 Pro 256GB', price: 1099, cost: 850, sku: 'IPH-15P-256', description: '6.1-inch display, A17 Pro chip, USB-C connector.' },
      { name: 'iPhone 15 Plus 128GB', price: 899, cost: 700, sku: 'IPH-15PL-128', description: '6.7-inch display, Dynamic Island, A16 Bionic chip.' },
      { name: 'iPhone 15 Plus 256GB', price: 999, cost: 800, sku: 'IPH-15PL-256', description: 'Large 6.7-inch display with extra storage capacity.' },
      { name: 'iPhone 15 128GB', price: 799, cost: 600, sku: 'IPH-15-128', description: 'Dynamic Island, 48MP Main camera, USB-C.' },
      { name: 'iPhone 15 256GB', price: 899, cost: 700, sku: 'IPH-15-256', description: 'Advanced dual-camera system, A16 Bionic chip.' },
      { name: 'iPhone 14 Pro Max 128GB', price: 1099, cost: 850, sku: 'IPH-14PM-128', description: 'Always-On display, Dynamic Island, 48MP Main camera.' },
      { name: 'iPhone 14 Pro 128GB', price: 999, cost: 750, sku: 'IPH-14P-128', description: '6.1-inch display, A16 Bionic, Crash Detection.' },
      { name: 'iPhone 14 Plus 128GB', price: 799, cost: 600, sku: 'IPH-14PL-128', description: 'Longest battery life ever, 6.7-inch display.' },
      { name: 'iPhone 14 128GB', price: 699, cost: 500, sku: 'IPH-14-128', description: 'Vibrant Super Retina XDR display, A15 Bionic.' },
      { name: 'iPhone 13 128GB', price: 599, cost: 450, sku: 'IPH-13-128', description: 'Dual-camera system, A15 Bionic chip, durable design.' },
      { name: 'iPhone 13 Mini 128GB', price: 499, cost: 350, sku: 'IPH-13M-128', description: 'Pocket-sized power, 5.4-inch display, A15 Bionic.' },
      { name: 'iPhone 12 64GB', price: 499, cost: 350, sku: 'IPH-12-64', description: 'A14 Bionic, 5G capable, Ceramic Shield front.' },
      { name: 'iPhone SE (3rd Gen) 64GB', price: 429, cost: 300, sku: 'IPH-SE3-64', description: 'Classic design, A15 Bionic chip, 5G connectivity.' },
      { name: 'iPhone SE (3rd Gen) 128GB', price: 479, cost: 340, sku: 'IPH-SE3-128', description: 'Touch ID, powerful chip, ample storage.' },
      { name: 'iPhone 11 64GB', price: 399, cost: 280, sku: 'IPH-11-64', description: 'Dual-camera system, Night mode, all-day battery.' },
      { name: 'iPhone 12 Pro 128GB Refurb', price: 550, cost: 400, sku: 'IPH-12P-REF', description: 'Certified Refurbished, Stainless steel design.' },
      { name: 'iPhone 12 Pro Max 128GB Refurb', price: 650, cost: 500, sku: 'IPH-12PM-REF', description: 'Certified Refurbished, Largest display.' },
      { name: 'iPhone XS Max 64GB Used', price: 300, cost: 200, sku: 'IPH-XSM-USED', description: 'Pre-owned, Super Retina OLED display.' },
      { name: 'iPhone XR 64GB Used', price: 250, cost: 150, sku: 'IPH-XR-USED', description: 'Pre-owned, Liquid Retina HD display.' },
      { name: 'iPhone 8 Plus 64GB Used', price: 200, cost: 120, sku: 'IPH-8P-USED', description: 'Pre-owned, Dual cameras with Portrait mode.' },
      { name: 'iPhone 16 Pro Max 256GB', price: 1199, cost: 950, sku: 'IPH-16PM-256', description: '6.9-inch display, A18 Pro chip, 48MP Ultra-Wide camera, Camera Control button.' },
      { name: 'iPhone 16 Pro 128GB', price: 999, cost: 800, sku: 'IPH-16P-128', description: '6.3-inch display, A18 Pro chip, 5x Telephoto zoom, Grade 5 Titanium design.' },
      { name: 'iPhone 16 Plus 128GB', price: 899, cost: 700, sku: 'IPH-16PL-128', description: '6.7-inch display, A18 chip, Camera Control, Apple Intelligence ready.' },
      { name: 'iPhone 16 128GB', price: 799, cost: 620, sku: 'IPH-16-128', description: '6.1-inch display, A18 chip, Action button, dual-camera system.' },
      { name: 'iPhone 16e 128GB', price: 599, cost: 450, sku: 'IPH-16E-128', description: '2025 budget model, A18 chip, single "2-in-1" camera, Apple Intelligence support.' }
   
    ];

    const iPads = [
      { name: 'iPad Pro 12.9 M2 128GB', price: 1099, cost: 850, sku: 'IPD-PRO12-M2-128', description: 'M2 chip, Liquid Retina XDR mini-LED display.' },
      { name: 'iPad Pro 12.9 M2 256GB', price: 1199, cost: 950, sku: 'IPD-PRO12-M2-256', description: 'Extreme performance, Apple Pencil hover.' },
      { name: 'iPad Pro 12.9 M2 512GB', price: 1399, cost: 1100, sku: 'IPD-PRO12-M2-512', description: 'Pro workflows, 512GB storage for creators.' },
      { name: 'iPad Pro 11 M2 128GB', price: 799, cost: 600, sku: 'IPD-PRO11-M2-128', description: 'M2 chip, 11-inch Liquid Retina display.' },
      { name: 'iPad Pro 11 M2 256GB', price: 899, cost: 700, sku: 'IPD-PRO11-M2-256', description: 'Portable pro performance, Face ID.' },
      { name: 'iPad Air 5 M1 64GB', price: 599, cost: 450, sku: 'IPD-AIR5-64', description: 'M1 chip, 10.9-inch Liquid Retina, 5G capable.' },
      { name: 'iPad Air 5 M1 256GB', price: 749, cost: 600, sku: 'IPD-AIR5-256', description: 'M1 power, Center Stage camera, 256GB.' },
      { name: 'iPad Mini 6 64GB', price: 499, cost: 380, sku: 'IPD-MINI6-64', description: '8.3-inch Liquid Retina, A15 Bionic, USB-C.' },
      { name: 'iPad Mini 6 256GB', price: 649, cost: 500, sku: 'IPD-MINI6-256', description: 'Small size, huge power, Apple Pencil 2 support.' },
      { name: 'iPad (10th Gen) 64GB', price: 449, cost: 350, sku: 'IPD-10-64', description: 'All-screen design, 10.9-inch display, A14 Bionic.' },
      { name: 'iPad (10th Gen) 256GB', price: 599, cost: 480, sku: 'IPD-10-256', description: 'Colorful design, Landscape Ultra Wide front camera.' },
      { name: 'iPad (9th Gen) 64GB', price: 329, cost: 250, sku: 'IPD-9-64', description: 'A13 Bionic, 10.2-inch Retina display, Touch ID.' },
      { name: 'iPad (9th Gen) 256GB', price: 479, cost: 380, sku: 'IPD-9-256', description: 'Essential iPad experience with extra storage.' },
      { name: 'iPad Pro 12.9 M1 128GB Refurb', price: 899, cost: 700, sku: 'IPD-PRO12-M1-REF', description: 'Refurbished M1 Powerhouse, XDR display.' },
      { name: 'iPad Pro 11 M1 128GB Refurb', price: 699, cost: 550, sku: 'IPD-PRO11-M1-REF', description: 'Refurbished M1 performance, ProMotion.' },
      { name: 'iPad Air 4 64GB Refurb', price: 450, cost: 350, sku: 'IPD-AIR4-REF', description: 'Refurbished A14 Bionic, All-screen design.' },
      { name: 'iPad Mini 5 64GB Used', price: 300, cost: 200, sku: 'IPD-MINI5-USED', description: 'Used condition, A12 Bionic, 7.9-inch display.' },
      { name: 'iPad Pro 12.9 (2020) Used', price: 600, cost: 450, sku: 'IPD-PRO12-2020', description: 'Used condition, A12Z Bionic, LiDAR Scanner.' },
      { name: 'iPad Pro 11 (2020) Used', price: 500, cost: 380, sku: 'IPD-PRO11-2020', description: 'Used condition, Face ID, ProMotion.' },
      { name: 'iPad (8th Gen) 32GB Used', price: 200, cost: 150, sku: 'IPD-8-USED', description: 'Used condition, A12 Bionic, Apple Pencil 1 support.' },
      { name: 'iPad Air 3 64GB Used', price: 250, cost: 180, sku: 'IPD-AIR3-USED', description: 'Used condition, A12 Bionic, Laminated display.' },
      { name: 'iPad Pro 10.5 Used', price: 220, cost: 150, sku: 'IPD-PRO105-USED', description: 'Used condition, A10X Fusion, ProMotion.' },
      { name: 'iPad Pro 9.7 Used', price: 180, cost: 120, sku: 'IPD-PRO97-USED', description: 'Used condition, 9.7-inch display, A9X.' }
    ];

    const laptops = [
      { name: 'MacBook Air M3 13"', price: 1099, cost: 850, sku: 'MAC-AIR-M3-13', description: 'M3 chip, 13.6-inch Liquid Retina, Midnight finish.' },
      { name: 'MacBook Air M3 15"', price: 1299, cost: 1000, sku: 'MAC-AIR-M3-15', description: 'M3 chip, 15.3-inch Liquid Retina, Super thin.' },
      { name: 'MacBook Pro 14 M3', price: 1599, cost: 1250, sku: 'MAC-PRO-14-M3', description: 'M3 chip, 14-inch XDR display, Space Gray.' },
      { name: 'MacBook Pro 16 M3 Pro', price: 2499, cost: 2000, sku: 'MAC-PRO-16-M3P', description: 'M3 Pro chip, 16-inch XDR, 18GB RAM.' },
      { name: 'MacBook Air M2 13"', price: 999, cost: 750, sku: 'MAC-AIR-M2-13', description: 'M2 chip, Redesigned chassis, MagSafe charging.' },
      { name: 'Dell XPS 13 Plus', price: 1399, cost: 1100, sku: 'DELL-XPS-13P', description: 'Intel Core i7, OLED touch, invisible touchpad.' },
      { name: 'Dell XPS 15 OLED', price: 1899, cost: 1500, sku: 'DELL-XPS-15', description: '3.5K OLED, RTX 4050, Intel Core i9.' },
      { name: 'ThinkPad X1 Carbon Gen 11', price: 1799, cost: 1400, sku: 'LEN-X1-G11', description: 'Ultralight business flagship, Carbon fiber chassis.' },
      { name: 'HP Spectre x360 14', price: 1499, cost: 1150, sku: 'HP-SPEC-14', description: '2-in-1 Convertible, OLED display, 9MP camera.' },
      { name: 'Asus ZenBook Duo 14', price: 1299, cost: 950, sku: 'ASUS-ZEN-14', description: 'Dual screen laptop for productivity.' },
      { name: 'Razer Blade 14', price: 2199, cost: 1800, sku: 'RAZ-BLD-14', description: 'AMD Ryzen 9, RTX 4070, 14-inch gaming powerhouse.' },
      { name: 'Microsoft Surface Laptop 5', price: 999, cost: 750, sku: 'MS-SURF-L5', description: '13.5-inch Alcantara keyboard, Intel Evo.' },
      { name: 'LG Gram 17', price: 1699, cost: 1300, sku: 'LG-GRAM-17', description: 'Ultra-lightweight 17-inch laptop, long battery.' },
      { name: 'Samsung Galaxy Book3 Pro', price: 1449, cost: 1100, sku: 'SAM-BOOK3-PRO', description: 'AMOLED 3K display, Intel 13th Gen, thin design.' },
      { name: 'Acer Swift 5', price: 1099, cost: 800, sku: 'ACER-SWF-5', description: 'Antimicrobial coating, Aerospace-grade aluminum.' },
      { name: 'MSI Stealth 16 Studio', price: 1999, cost: 1600, sku: 'MSI-ST-16', description: 'Gaming and Creator laptop, RTX 4060, 240Hz.' },
      { name: 'Lenovo Yoga 9i', price: 1399, cost: 1100, sku: 'LEN-YOGA-9I', description: 'Rotating soundbar, 4K OLED, 2-in-1 design.' },
      { name: 'Alienware x14', price: 1799, cost: 1400, sku: 'ALN-X14', description: 'Thinnest Alienware, RTX 4060, 165Hz display.' },
      { name: 'MacBook Pro 13 M2 Refurb', price: 1099, cost: 850, sku: 'MAC-PRO13-M2-REF', description: 'Refurbished M2, Touch Bar, active cooling.' },
      { name: 'Dell Inspiron 15', price: 599, cost: 450, sku: 'DELL-INS-15', description: 'Budget friendly, AMD Ryzen 5, 15.6-inch.' },
      { name: 'HP Pavilion 15', price: 649, cost: 500, sku: 'HP-PAV-15', description: 'Reliable daily driver, Bang & Olufsen audio.' },
      { name: 'Asus Vivobook 15', price: 499, cost: 380, sku: 'ASUS-VIVO-15', description: 'Thin and light, Intel Core i3, NanoEdge display.' },
      { name: 'Lenovo IdeaPad 3', price: 449, cost: 350, sku: 'LEN-IDEA-3', description: 'Entry level, 15.6-inch FHD, privacy shutter.' },
      { name: 'MacBook Pro 14 M4', price: 1599, cost: 1280, sku: 'MAC-PRO-14-M4', description: 'Latest M4 chip, Nano-texture display option, Space Black.' },
      { name: 'Dell XPS 14 (2024)', price: 1499, cost: 1200, sku: 'DELL-XPS-14-9440', description: 'Intel Core Ultra 7, CNC Aluminum, Gorilla Glass 3.' },
      { name: 'HP OmniBook Ultra Flip', price: 1449, cost: 1150, sku: 'HP-OMNI-FLIP', description: 'AI-ready 2-in-1, 3K OLED, haptic touchpad.' },
      { name: 'ASUS ROG Zephyrus G14', price: 1599, cost: 1300, sku: 'ASUS-ROG-G14-24', description: 'OLED Nebula Display, RTX 4060, ultra-portable gaming.' },
      { name: 'Surface Laptop 7th Ed', price: 999, cost: 780, sku: 'MS-SURF-L7', description: 'Snapdragon X Elite, Copilot+ PC, 20-hour battery.' }
    ];

    const tablets = [
      { name: 'Samsung Galaxy Tab S9 Ultra', price: 1199, cost: 900, sku: 'SAM-S9-ULT', description: '14.6-inch Dynamic AMOLED 2X, S Pen included.' },
      { name: 'Samsung Galaxy Tab S9+', price: 999, cost: 750, sku: 'SAM-S9-PLS', description: '12.4-inch AMOLED, Snapdragon 8 Gen 2.' },
      { name: 'Samsung Galaxy Tab S9', price: 799, cost: 600, sku: 'SAM-S9', description: '11-inch AMOLED, IP68 water resistance.' },
      { name: 'Samsung Galaxy Tab S9 FE', price: 449, cost: 340, sku: 'SAM-S9-FE', description: 'Fan Edition, 10.9-inch LCD, 90Hz.' },
      { name: 'Google Pixel Tablet', price: 499, cost: 380, sku: 'GOO-PIX-TAB', description: 'Includes Charging Speaker Dock, Tensor G2.' },
      { name: 'OnePlus Pad', price: 479, cost: 360, sku: 'ONE-PAD', description: '11.61-inch 144Hz display, 67W fast charging.' },
      { name: 'Lenovo Tab P12 Pro', price: 699, cost: 500, sku: 'LEN-P12-PRO', description: '12.6-inch AMOLED 120Hz, Snapdragon 870.' },
      { name: 'Xiaomi Pad 6', price: 399, cost: 280, sku: 'XIA-PAD-6', description: '11-inch WQHD+ 144Hz, Snapdragon 870.' },
      { name: 'Amazon Fire Max 11', price: 229, cost: 150, sku: 'AMZ-FIRE-11', description: '11-inch 2K display, 14 hour battery life.' },
      { name: 'Samsung Galaxy Tab A9+', price: 219, cost: 160, sku: 'SAM-A9-PLS', description: '11-inch 90Hz display, Quad speakers.' },
      { name: 'Lenovo Tab M10 Plus', price: 189, cost: 130, sku: 'LEN-M10-PLS', description: '10.6-inch 2K IPS, Reading mode.' },
      { name: 'Samsung Galaxy Tab S8 Ultra Refurb', price: 899, cost: 650, sku: 'SAM-S8-ULT-REF', description: 'Refurbished 14.6-inch giant tablet.' },
      { name: 'Samsung Galaxy Tab S8+ Refurb', price: 699, cost: 500, sku: 'SAM-S8-PLS-REF', description: 'Refurbished Super AMOLED display.' },
      { name: 'Microsoft Surface Pro 9', price: 999, cost: 750, sku: 'MS-SURF-P9', description: '2-in-1, Intel Core i5, 13-inch PixelSense.' },
      { name: 'Microsoft Surface Go 3', price: 399, cost: 300, sku: 'MS-SURF-G3', description: 'Portable 10.5-inch touchscreen 2-in-1.' },
      { name: 'Chuwi HiPad XPro', price: 149, cost: 100, sku: 'CHU-HIPAD', description: 'Budget Android 12, 10.5-inch FHD.' },
      { name: 'Teclast T50', price: 199, cost: 140, sku: 'TEC-T50', description: '2K display, Unibody aluminum design.' },
      { name: 'Nokia T21', price: 239, cost: 170, sku: 'NOK-T21', description: 'Tough built, 10.4-inch 2K display, 3 days battery.' },
      { name: 'Realme Pad 2', price: 249, cost: 180, sku: 'REA-PAD-2', description: '11.5-inch 120Hz 2K display, 33W charge.' },
      { name: 'Oppo Pad Air', price: 299, cost: 220, sku: 'OPP-PAD-A', description: 'Ultra thin 6.9mm, Snapdragon 680.' },
      { name: 'Vivo Pad 2', price: 399, cost: 300, sku: 'VIV-PAD-2', description: '12.1-inch 144Hz display, Dimensity 9000.' },
      { name: 'Huawei MatePad Pro 13.2', price: 999, cost: 750, sku: 'HUA-MATE-13', description: 'Flexible OLED, Near-Link M-Pencil.' },
      { name: 'Honor Pad 9', price: 349, cost: 250, sku: 'HON-PAD-9', description: '12.1-inch Paper-like display protection.' }
    ];

    const pcs = [
      { name: 'Mac Mini M2', price: 599, cost: 450, sku: 'MAC-MINI-M2', description: 'M2 chip, 8-core CPU, 10-core GPU.' },
      { name: 'Mac Mini M2 Pro', price: 1299, cost: 1000, sku: 'MAC-MINI-M2P', description: 'M2 Pro chip, 4x Thunderbolt 4 ports.' },
      { name: 'Mac Studio M2 Max', price: 1999, cost: 1600, sku: 'MAC-STUDIO-M2', description: 'Compact powerhouse for creative pros.' },
      { name: 'Mac Studio M2 Ultra', price: 3999, cost: 3200, sku: 'MAC-STUDIO-ULT', description: 'The most powerful Mac silicon ever.' },
      { name: 'iMac 24" M3', price: 1299, cost: 1000, sku: 'IMAC-24-M3', description: 'All-in-one, 4.5K Retina display, Color-matched accessories.' },
      { name: 'Dell XPS Desktop', price: 1499, cost: 1100, sku: 'DELL-XPS-DT', description: 'Minimalist design, Liquid cooling, RTX 4070.' },
      { name: 'Alienware Aurora R16', price: 1799, cost: 1400, sku: 'ALN-AUR-R16', description: 'Optimized airflow, Legend 3 design, Core i7.' },
      { name: 'HP Omen 45L', price: 2499, cost: 2000, sku: 'HP-OMEN-45', description: 'Cryo Chamber cooling, RTX 4080, Glass side panel.' },
      { name: 'Lenovo Legion Tower 7i', price: 2299, cost: 1800, sku: 'LEN-LEG-T7', description: 'Coldfront 4.0 cooling, Core i9 K-series.' },
      { name: 'Corsair Vengeance i7400', price: 2899, cost: 2300, sku: 'COR-VEN-I7', description: 'High-airflow case, iCUE RGB lighting.' },
      { name: 'MSI Aegis RS', price: 1899, cost: 1500, sku: 'MSI-AEGIS', description: 'Esports ready, Standard components for upgradability.' },
      { name: 'Skytech Azure Gaming PC', price: 1599, cost: 1200, sku: 'SKY-AZU-GM', description: 'RTX 4070, 1TB NVMe, Mesh front panel.' },
      { name: 'CyberPowerPC Gamer Supreme', price: 1999, cost: 1600, sku: 'CYB-GAM-SUP', description: 'Liquid Cooled, 32GB DDR5, Custom RGB.' },
      { name: 'iBuyPower SlateMesh', price: 1399, cost: 1000, sku: 'IBP-SLA-MSH', description: 'High airflow mesh, RTX 4060 Ti.' },
      { name: 'Intel NUC 13 Extreme', price: 1499, cost: 1150, sku: 'INT-NUC-13', description: 'Tiny footprint, Supports full-size GPU.' },
      { name: 'HP Envy Desktop', price: 899, cost: 650, sku: 'HP-ENVY-DT', description: 'Content creator focused, plenty of ports.' },
      { name: 'Dell Inspiron Desktop', price: 699, cost: 500, sku: 'DELL-INS-DT', description: 'Compact home office PC, Intel Core i5.' },
      { name: 'Acer Predator Orion 7000', price: 2999, cost: 2400, sku: 'ACER-PRE-7K', description: 'FrostBlade fans, ARGB, RTX 4090 beast.' },
      { name: 'NZXT Player Three', price: 2499, cost: 2000, sku: 'NZXT-PL3', description: 'H9 Flow case, Kraken Elite cooler, RTX 4080.' },
      { name: 'Maingear MG-1', price: 1699, cost: 1300, sku: 'MAIN-MG1', description: 'Compact chassis, Customizable magnetic front panel.' },
      { name: 'Origin PC Neuron', price: 2599, cost: 2100, sku: 'ORG-NEURON', description: 'Custom boutique build, UV printed glass option.' },
      { name: 'Asus ROG Strix G16CH', price: 1799, cost: 1400, sku: 'ASUS-ROG-DT', description: 'Headphone holder, Carry handle, Airflow focused.' },
    ];

    const tvs = [
      { name: 'LG C3 OLED 55"', price: 1499, cost: 1100, sku: 'LG-C3-55', description: 'OLED evo, Alpha 9 Gen 6 Processor, Dolby Vision.' },
      { name: 'LG C3 OLED 65"', price: 1899, cost: 1400, sku: 'LG-C3-65', description: 'Perfect black, 120Hz refresh rate for gaming.' },
      { name: 'LG G3 OLED 65"', price: 2799, cost: 2100, sku: 'LG-G3-65', description: 'Brightness Booster Max, One Wall Design.' },
      { name: 'Samsung S90C OLED 55"', price: 1599, cost: 1200, sku: 'SAM-S90C-55', description: 'QD-OLED technology, LaserSlim design.' },
      { name: 'Samsung S90C OLED 65"', price: 1999, cost: 1500, sku: 'SAM-S90C-65', description: 'Pantone Validated colors, Motion Xcelerator.' },
      { name: 'Samsung QN90C Neo QLED 65"', price: 2199, cost: 1700, sku: 'SAM-QN90C-65', description: 'Mini LED, Anti-Glare, Ultra Viewing Angle.' },
      { name: 'Sony Bravia XR A80L 55"', price: 1699, cost: 1300, sku: 'SONY-A80L-55', description: 'Cognitive Processor XR, Acoustic Surface Audio.' },
      { name: 'Sony Bravia XR A95L 65"', price: 3499, cost: 2800, sku: 'SONY-A95L-65', description: 'QD-OLED flagship, XR Triluminos Max.' },
      { name: 'TCL QM8 Mini-LED 65"', price: 1199, cost: 900, sku: 'TCL-QM8-65', description: '2000+ local dimming zones, 2000 nits peak brightness.' },
      { name: 'Hisense U8K Mini-LED 65"', price: 1099, cost: 850, sku: 'HIS-U8K-65', description: '144Hz native refresh, Wi-Fi 6E.' },
      { name: 'Vizio P-Series Quantum 65"', price: 999, cost: 750, sku: 'VIZ-PQ-65', description: 'QLED color, Active Full Array backlight.' },
      { name: 'Roku Plus Series TV 55"', price: 499, cost: 380, sku: 'ROKU-PLS-55', description: 'Built-in Roku OS, QLED, Voice Remote Pro.' },
      { name: 'Amazon Fire TV Omni 65"', price: 599, cost: 450, sku: 'AMZ-OMNI-65', description: 'Hands-free Alexa, QLED, Local Dimming.' },
      { name: 'LG B3 OLED 55"', price: 1199, cost: 900, sku: 'LG-B3-55', description: 'Entry-level OLED, 120Hz, G-Sync compatible.' },
      { name: 'Samsung The Frame 55"', price: 1499, cost: 1100, sku: 'SAM-FRAME-55', description: 'Art Mode, Matte Display, Slim Fit Wall Mount.' },
      { name: 'Sony X90L Full Array 65"', price: 1299, cost: 1000, sku: 'SONY-X90L-65', description: 'Full Array LED, Perfect for PS5.' },
      { name: 'TCL 6-Series 55"', price: 699, cost: 500, sku: 'TCL-6S-55', description: 'Mini-LED technology, Roku TV built-in.' },
      { name: 'Hisense U7K 55"', price: 799, cost: 600, sku: 'HIS-U7K-55', description: 'Gaming TV, 144Hz, Game Bar.' },
      { name: 'Samsung CU7000 43"', price: 299, cost: 220, sku: 'SAM-CU7-43', description: 'Crystal UHD 4K, Smart Tizen OS.' },
      { name: 'LG UR9000 50"', price: 399, cost: 300, sku: 'LG-UR9-50', description: '4K UHD, HDR10 Pro, Magic Remote.' },
      { name: 'Sony X80K 43"', price: 449, cost: 340, sku: 'SONY-X80K-43', description: '4K HDR Processor X1, Triluminos Pro.' },
      { name: 'Insignia F30 Fire TV 50"', price: 249, cost: 180, sku: 'INS-F30-50', description: 'DTS Studio Sound, Voice Remote.' },
      { name: 'Toshiba C350 43"', price: 229, cost: 170, sku: 'TOSH-C35-43', description: 'Regza Engine 4K, Bezel-less design.' },
      { name: 'LG C4 OLED 55"', price: 1899, cost: 1450, sku: 'LG-C4-55', description: 'α9 AI Processor Gen7, 144Hz refresh rate, Filmmaker Mode for Dolby Vision.' },
      { name: 'Samsung S95D OLED 65"', price: 2399, cost: 1850, sku: 'SAM-S95D-65', description: 'Glare-free OLED technology, NQ4 AI Gen2 Processor, up to 144Hz refresh rate.' },
      { name: 'Sony Bravia 9 Mini-LED 65"', price: 1999, cost: 1550, sku: 'SONY-B9-65', description: 'Sony’s brightest 4K TV, XR Backlight Master Drive, High Peak Brightness QLED.' },
      { name: 'TCL QM851G Mini-LED 75"', price: 1499, cost: 1100, sku: 'TCL-QM851-75', description: 'High-end Mini-LED with 5000+ nits peak brightness and 144Hz VRR.' }
    ];

    const productsToSeed = [
      { category: 'iPhone', items: iPhones },
      { category: 'iPad', items: iPads },
      { category: 'Laptop', items: laptops },
      { category: 'Tablet', items: tablets },
      { category: 'PC', items: pcs },
      { category: 'TV', items: tvs },
    ];

    const allCreatedProducts: Product[] = [];

    for (const group of productsToSeed) {
      const category = categories.find(c => c.name === group.category);
      if (!category) continue;

      for (const item of group.items) {
        const product = await prisma.product.create({
          data: {
            name: item.name,
            sku: item.sku,
            barcode: Math.floor(Math.random() * 1000000000000).toString(),
            price: new Prisma.Decimal(item.price),
            costPrice: new Prisma.Decimal(item.cost),
            stock: Math.floor(Math.random() * 50) + 5,
            minStock: 5,
            categoryId: category.id,
            description: item.description,
          },
        });
        allCreatedProducts.push(product);
      }
    }
    console.log(`Created ${allCreatedProducts.length} total products`);

    // 5. Customers (UPDATED: Western/International Names)
    console.log('Seeding customers...');
    const customerData = [
      { name: 'John Smith', email: 'john.smith@example.com', phone: '0901234567', address: '123 Main St, New York, NY', isMember: true, memberSince: new Date('2024-01-01') },
      { name: 'Emily Johnson', email: 'emily.j@example.com', phone: '0902345678', address: '456 Market St, San Francisco, CA', isMember: false },
      { name: 'Michael Williams', email: 'mike.w@example.com', phone: '0903456789', address: '789 Broadway, Los Angeles, CA', isMember: true, memberSince: new Date('2024-06-15') },
      { name: 'Sarah Brown', email: 'sarah.b@example.com', phone: '0904567890', address: '321 Elm St, Chicago, IL', isMember: false },
      { name: 'David Jones', email: 'david.j@example.com', phone: '0905678901', address: '654 Pine St, Houston, TX', isMember: true, memberSince: new Date('2024-03-20') },
      { name: 'Jennifer Garcia', email: 'jen.garcia@example.com', phone: '0906789012', address: '987 Oak Ave, Miami, FL', isMember: true, memberSince: new Date('2024-02-10') },
      { name: 'Robert Miller', email: 'rob.miller@example.com', phone: '0907890123', address: '147 Maple Dr, Seattle, WA', isMember: false },
      { name: 'Jessica Davis', email: 'jess.davis@example.com', phone: '0908901234', address: '258 Cedar Ln, Boston, MA', isMember: false },
      { name: 'William Rodriguez', email: 'will.rod@example.com', phone: '0909012345', address: '369 Birch Rd, Denver, CO', isMember: true, memberSince: new Date('2024-05-01') },
      { name: 'Elizabeth Martinez', email: 'liz.martinez@example.com', phone: '0900123456', address: '741 Spruce Ct, Atlanta, GA', isMember: true, memberSince: new Date('2024-07-20') },
      { name: 'James Anderson', email: 'james.a@example.com', phone: '0911234567', address: '852 Willow Way, Phoenix, AZ', isMember: false },
      { name: 'Linda Taylor', email: 'linda.t@example.com', phone: '0912345678', address: '963 Aspen Pl, Portland, OR', isMember: false },
    ];

    const customers: Customer[] = [];
    for (const cus of customerData) {
      const c = await prisma.customer.create({ data: cus });
      customers.push(c);
    }
    console.log(`Created ${customers.length} customers`);

    // 6. Orders (UPDATED Loop to 100 for more records)
    console.log('Seeding orders...');
    if (allCreatedProducts.length > 0 && customers.length > 0) {
      const getRandomProduct = () => allCreatedProducts[Math.floor(Math.random() * allCreatedProducts.length)];
      const getRandomCustomer = () => customers[Math.floor(Math.random() * customers.length)];
      const getRandomUser = () => users[Math.floor(Math.random() * users.length)];
      
      const getRandomDate = (start: Date, end: Date) => {
        return new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
      };

      const statuses = ['COMPLETED', 'COMPLETED', 'COMPLETED', 'PROCESSING', 'PENDING', 'CANCELLED'];
      const numberOfOrdersToSeed = 100; // Increased to 100

      for (let i = 1; i <= numberOfOrdersToSeed; i++) {
        const orderDate = getRandomDate(new Date('2024-01-01'), new Date());
        const orderNum = `ORD-2024-${i.toString().padStart(3, '0')}`;
        const status = statuses[Math.floor(Math.random() * statuses.length)];
        
        const numItems = Math.floor(Math.random() * 4) + 1; 
        const items = [];
        
        for(let j = 0; j < numItems; j++) {
            const prod = getRandomProduct();
            items.push({ 
                productId: prod.id, 
                quantity: Math.floor(Math.random() * 2) + 1, 
                unitPrice: prod.price 
            });
        }

        const subtotal = items.reduce((sum, item) => sum + Number(item.unitPrice) * item.quantity, 0);
        
        await prisma.order.create({
          data: {
            orderNumber: orderNum,
            customerId: getRandomCustomer().id,
            userId: getRandomUser().id,
            status: status as any,
            subtotal: new Prisma.Decimal(subtotal),
            discountAmount: new Prisma.Decimal(0),
            taxAmount: new Prisma.Decimal(0),
            total: new Prisma.Decimal(subtotal),
            createdAt: orderDate,
            orderItems: {
              create: items.map(i => ({
                productId: i.productId,
                quantity: i.quantity,
                unitPrice: i.unitPrice,
                subtotal: new Prisma.Decimal(Number(i.unitPrice) * i.quantity),
                discountAmount: new Prisma.Decimal(0),
                total: new Prisma.Decimal(Number(i.unitPrice) * i.quantity),
              }))
            }
          }
        });
      }
      console.log(`Created ${numberOfOrdersToSeed} orders`);
    }

    // 7. License
    console.log('Seeding license keys...');
    await prisma.appLicense.create({
      data: {
        licenseKey: 'MYSHOP-TRIAL-0001',
        activatedAt: new Date(),
        expiresAt: new Date(new Date().getTime() + 15 * 24 * 60 * 60 * 1000),
        isActive: true,
      },
    });

    console.log('Database seeding completed successfully!');
  } catch (error) {
    console.error('Error seeding database:', error);
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

seed();