import { PrismaClient, Prisma, Product, Customer, Category, Discount } from '@prisma/client';
import { AuthUtils } from './auth';

const prisma = new PrismaClient();

// ==============================================================================
// 1. IMAGE MAPPING (SKU -> URL)
// ==============================================================================
const productImages: Record<string, string> = {
  // --- iPhone 15 Series ---
  'IPH-15PM-256': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_3.png',
  'IPH-15PM-512': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_5.png',
  'IPH-15PM-1TB': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_2__5_2_1_1_1_1_2_1_1.jpg',
  'IPH-15P-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-plus_1_.png',
  'IPH-15P-256': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_2__5_2_1_1_1_1_2_1_1.jpg',
  'IPH-15PL-128': 'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2023_9_13_638302007249847040_iPhone_15_Plus_Blue_Pure_Back_iPhone_15_Plus_Blue_Pure_Front_2up_Screen__USEN.jpg',
  'IPH-15PL-256': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone_15/dien-thoai-iphone-15-plus-256gb-3.jpg',
  'IPH-15-128': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/v/n/vn_iphone_15_yellow_pdp_image_position-1a_yellow_color_1_4_1_1.jpg',
  'IPH-15-256': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone_15/dien-thoai-iphone-15-256gb-8.jpg',

  // --- iPhone 14 Series ---
  'IPH-14PM-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/_/t_m_18_1_3_2.png',
  'IPH-14P-128': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/v/_/v_ng_12_1_2_1.png',
  'IPH-14PL-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/p/h/photo_2022-09-28_21-58-51_4_1_2_2.jpg',
  'IPH-14-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/p/h/photo_2022-09-28_21-58-56_11_1.jpg',

  // --- iPhone 13 & Older ---
  'IPH-13-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-13_2_2.jpg',
  'IPH-13M-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/1/4/14_1_9_2_6.jpg',
  'IPH-12-64': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-12.png',
  'IPH-SE3-64': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/1/_/1_359_1.png',
  'IPH-SE3-128': 'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2022_4_15_637856361035158510_iPhone%20SE%20(8).jpg',
  'IPH-11-64': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-11.png',

  // --- Used / Refurbished ---
  'IPH-12P-REF': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/d/o/download_4_2_2.png',
  'IPH-12PM-REF': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/d/o/download_2__1_27.png',
  'IPH-XSM-USED': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone_xs_max_512gb_1_1.jpg',
  'IPH-XR-USED': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone_xr_64gb_1.png',
  'IPH-8P-USED': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone8-plus-silver-select-2018_6_3.png',

  // --- iPhone 16 Series ---
  'IPH-16PM-256': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/p/h/photo_2024-10-02_13-59-00_1.jpg',
  'IPH-16P-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-pro_1.png',
  'IPH-16PL-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-plus-1.png',
  'IPH-16-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-1.png',
  'IPH-16E-128': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16e-128gb.png',

  // --- iPads ---
  'IPD-PRO12-M2-128': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-pro-13-select-wifi-spacegray-202210-02_3_3_1_1_1_4.jpg',
  'IPD-PRO12-M2-256': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-13-select-202210_3_1.png',
  'IPD-PRO12-M2-512': 'https://cdn.tgdd.vn/Products/Images/522/295464/ipad-pro-m2-12.5-wifi-xam-thumb-600x600.jpg',
  'IPD-PRO11-M2-128': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-pro-13-select-wifi-silver-202210-01_4.jpg',
  'IPD-PRO11-M2-256': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-13-select-202210_1_1_1.png',
  'IPD-AIR5-64': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-air-5.png',
  'IPD-AIR5-256': 'https://cdn.tgdd.vn/Products/Images/522/274154/ipad-air-5-wifi-blue-thumb-1-600x600.jpg',
  'IPD-MINI6-64': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/_/t_i_xu_ng_2__1_8_1_1.png',
  'IPD-MINI6-256': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-mini-6-5_1_1_1_1.jpg',
  'IPD-10-64': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-10-9-inch-2022.png',
  'IPD-10-256': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-2022-hero-blue-wifi-select_1.png',
  'IPD-9-64': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/2/c/2c_v.png',
  'IPD-9-256': 'https://bizweb.dktcdn.net/thumb/1024x1024/100/401/951/products/dacdiemnoibatad7358efe2ed47aa9-6fd11bbc-2a77-4216-b94b-08369a6a8e34.png?v=1749147182043',
  'IPD-PRO12-M1-REF': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-pro-12-9-2021_1_1_2_1_1_2.jpg',
  'IPD-PRO11-M1-REF': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-11-2021-2_1_1_1_1_1_1_1_1_1_1_1_1_1_1.jpg',
  'IPD-AIR4-REF': 'https://ttcenter.com.vn/uploads/product/zsr37jz9-364-ipad-air-4-10-9-inch-wifi-64gb-like-new.jpg',
  'IPD-MINI5-USED': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-mini-select-wifi-silver-201903_7_1.png',
  'IPD-PRO12-2020': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/a/p/apple-ipad-pro-11-2020-wifi-256-gb-2_2_1_1.jpg',
  'IPD-PRO11-2020': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad_pro_11_2020_bac_3_1_2.jpg',
  'IPD-8-USED': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/1/9/19268_ipadgen8sliver_ok_3.jpg',
  'IPD-AIR3-USED': 'https://hoangtrungmobile.vn/wp-content/uploads/2021/03/ipad-air-3-bac.jpg',
  'IPD-PRO105-USED': 'https://didongthongminh.vn/images/products/2025/09/19/original/2(4).jpg',
  'IPD-PRO97-USED': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-9in-gold_3_1_2_1.jpg',

  // --- Laptops ---
  'MAC-AIR-M3-13': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/m/b/mba13-m3-midnight-gallery1-202402_3_1_2_1.jpg',
  'MAC-AIR-M3-15': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/laptop/macbook/Air/M3-2024/macbook-air-m3-15-inch-2024-1_1.jpg',
  'MAC-PRO-14-M3': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/laptop/macbook/macbook-pro-7.jpg',
  'MAC-PRO-16-M3P': 'https://bizweb.dktcdn.net/100/318/659/products/7-5581399e-3267-456a-8d4f-9259ac8f5dc0.jpg?v=1699430082770',
  'MAC-AIR-M2-13': 'https://cdn2.fptshop.com.vn/unsafe/macbook_air_13_m2_midnight_1_35053fbcf9.png',
  'DELL-XPS-13P': 'https://cellphones.com.vn/sforum/wp-content/uploads/2022/01/tren-tay-Dell-XPS-13-Plus-13.jpg',
  'DELL-XPS-15': 'https://media.wired.com/photos/6169f03b58660fcbc5f4ec3b/master/w_1600%2Cc_limit/Gear-Dell-XPS-15-OLED-1.jpg',
  'LEN-X1-G11': 'https://cdn-media.sforum.vn/storage/app/media/wp-content/uploads/2023/11/10-6.jpg',
  'HP-SPEC-14': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/l/a/laptop-hp-spectre-x360-14-ea0023xd-cu-dep-3_1.jpg',
  'ASUS-ZEN-14': 'https://nguyencongpc.vn/media/product/20151-asus-zenbook-duo-14-ux482eg-ka166t-6.jpg',
  'RAZ-BLD-14': 'https://laptops.vn/wp-content/uploads/2024/06/razer-blade-14-2024-1710487813_1711595620-1.jpg',
  'MS-SURF-L5': 'https://vhost53003.vhostcdn.com/wp-content/uploads/2022/10/microsoft-surface-laptop-5-2.jpg',
  'LG-GRAM-17': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/e/text_ng_n_55__2_11.png',
  'SAM-BOOK3-PRO': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/3/6/360pro_1.png',
  'ACER-SWF-5': 'https://cellphones.com.vn/sforum/wp-content/uploads/2020/06/Acer-Swift-5-SF514-55-Standard_01.png',
  'MSI-ST-16': 'https://product.hstatic.net/200000722513/product/057vn_9630c86ceec944c49425ef01bb5c879d_master.png',
  'LEN-YOGA-9I': 'https://cdn-media.sforum.vn/storage/app/media/chidung/yoga-9i/danh-gia-yoga-9i-2024-14.jpg',
  'ALN-X14': 'https://www.laptopvip.vn/images/ab__webp/detailed/32/notebook-alienware-x14-r2-gray-gallery-6-www.laptopvip.vn-1686985486.webp',
  'MAC-PRO13-M2-REF': 'https://product.hstatic.net/200000768357/product/gray_fab6e9b0c7374bfd86b9189632447680.png',
  'DELL-INS-15': 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/330075/dell-inspiron-15-3520-i5-n3520-i5u085w11slu-1-638627942653445825-750x500.jpg',
  'HP-PAV-15': 'https://www.laptopvip.vn/images/companies/1/JVS/HP/HP-Pavilion-15T/71lc66S1jqL._AC_SL1500_.jpg?1666681642506',
  'ASUS-VIVO-15': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/e/text_ng_n_1__5_16_1.png',
  'LEN-IDEA-3': 'https://p3-ofp.static.pub/fes/cms/2022/12/28/cbsimp9kdhc8w1tw2t7pytz6exsvvv545729.jpg',
  'MAC-PRO-14-M4': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/laptop/macbook/macbook-pro-7.jpg',
  'DELL-XPS-14-9440': 'https://hungphatlaptop.com/wp-content/uploads/2024/01/DELL-XPS-14-9440-2024-Platinum-H1-1.jpeg',
  'HP-OMNI-FLIP': 'https://cdn2.fptshop.com.vn/unsafe/800x0/hp_omnibook_ultra_flip_14_fh0040tu_b13vhpa_6_2a358fe388.jpg',
  'ASUS-ROG-G14-24': 'https://dlcdnwebimgs.asus.com/gain/E90DE227-7002-48C1-A940-B6E952D0BCCC',
  'MS-SURF-L7': 'https://surfaceviet.vn/wp-content/uploads/2024/05/Surface-Laptop-7-Black-15-inch.jpg',

  // --- Tablets ---
  'SAM-S9-ULT': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tab-s9-ultra-kem-2_1_1.png',
  'SAM-S9-PLS': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/e/p/eprice_1_b7620c148ab010a64546e96a356978b2_2_1.jpg',
  'SAM-S9': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/s/ss-tab-s9_1.png',
  'SAM-S9-FE': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tab-s9-fe-xam_2_1_1.png',
  'GOO-PIX-TAB': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/g/o/google_pixel_tablet.jpg',
  'ONE-PAD': 'https://cdn.viettablet.com/images/thumbnails/480/516/detailed/56/oneplus-pad-chinh-hang.jpg',
  'LEN-P12-PRO': 'https://p4-ofp.static.pub/fes/cms/2023/03/28/7dch8vg9lz0tzeg74u3x9paoln4o8z319478.png',
  'XIA-PAD-6': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/i/mi-pad-6-cps-doc-quyen-xanh_3_1_1.jpg',
  'AMZ-FIRE-11': 'https://product.hstatic.net/200000730863/product/51gj5oqxbnl._ac_sl1000__f75a5ec479ef4fc1ac2f78b17c6da98d_master.jpg',
  'SAM-A9-PLS': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-tab-a9_11__1.png',
  'LEN-M10-PLS': 'https://p2-ofp.static.pub/fes/cms/2023/03/29/8rz4mn5wzzx3s29zfcffctkb2xcwjj602719.png',
  'SAM-S8-ULT-REF': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/h/t/https___bucketeer_e05bbc84_baa3_437e_9518_adb32be77984.s3.amazonaws.com_public_images_b08df22d_4b5e_46a8_87c5_fc303e133f8a_1500x1500_1_1_1_1.jpg',
  'SAM-S8-PLS-REF': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/e/series_tab_s8001_1_2.jpg',
  'MS-SURF-P9': 'https://surfacecity.vn/wp-content/uploads/microsoft-surface-pro-9-5g.jpg',
  'MS-SURF-G3': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/5/6/5650372_surface_go_3_under_embargo_until_22.jpg',
  'CHU-HIPAD': 'https://www.chuwi.com/public/upload/image/20221229/52bc2a3d58a50a2bb14171419cc30094.png',
  'TEC-T50': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/may-tinh-bang-teclast-t50-plus_1_.png',
  'NOK-T21': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/n/o/nokia-t21_12_.png',
  'REA-PAD-2': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/r/e/realme-pad-2.png',
  'OPP-PAD-A': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/o/p/oppo-pad-air-128gb.jpg',
  'VIV-PAD-2': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/v/i/vivo-pad-2_2_.jpg',
  'HUA-MATE-13': 'https://cellphones.com.vn/sforum/wp-content/uploads/2023/10/Huawei-Matepad-Pro-13.9-3.jpg',
  'HON-PAD-9': 'https://cdn2.cellphones.com.vn/x/media/catalog/product/m/a/may-tinh-bang-honor-pad-9-pro_3_.png',

  // --- PCs ---
  'MAC-MINI-M2': 'https://mac24h.vn/images/companies/1/12inch%20rose/Macbook%2012%20inch%20gold/macminipost.png?1598064081104',
  'MAC-MINI-M2P': 'https://cdn2.cellphones.com.vn/200x/media/catalog/product/m/a/macbook_33_.png',
  'MAC-STUDIO-M2': 'https://product.hstatic.net/200000348419/product/mac_studio_m2_max_2023_chinh_hang_21aed22940d54b5f8c6bc1e92f721ab1_large.png',
  'MAC-STUDIO-ULT': 'https://macstores.vn/wp-content/uploads/2023/06/mac-studio-m2-4.jpg',
  'IMAC-24-M3': 'https://shopdunk.com/images/thumbs/0022756_imac-m3-2023-24-inch-8-core-gpu8gb256gb.jpeg',
  'DELL-XPS-DT': 'https://www.laptopvip.vn/images/ab__webp/detailed/10/dell-xps-27.webp',
  'ALN-AUR-R16': 'https://images-na.ssl-images-amazon.com/images/I/71C+ewM2JjL.jpg',
  'HP-OMEN-45': 'https://kaas.hpcloud.hp.com/PROD/v2/renderbinary/7477130/5038347/con-win-dt-p-omen-45l-gt22-1009nf-product-specifications/articuno-desktop',
  'LEN-LEG-T7': 'https://p2-ofp.static.pub//fes/cms/2024/11/27/em8bpvjffmescc7mck5snjp1g73otp127407.png',
  'COR-VEN-I7': 'https://res.cloudinary.com/corsair-pwa/image/upload/v1684950787/products/Vengeance-PC/CS-9050047-NA/Gallery/common/Vengeance__i7400_01.webp',
  'MSI-AEGIS': 'https://asset.msi.com/resize/image/global/product/product_1669160633809914962a2cb40d02df74877b17555b.png62405b38c58fe0f07fcef2367d8a9ba1/1024.png',
  'SKY-AZU-GM': 'https://m.media-amazon.com/images/I/71gtoidr0kL._AC_UF894,1000_QL80_.jpg',
  'CYB-GAM-SUP': 'https://m.media-amazon.com/images/I/818SNa1ruZL.jpg',
  'IBP-SLA-MSH': 'https://m.media-amazon.com/images/I/81xlNPKMrQL._AC_UF1000,1000_QL80_.jpg',
  'INT-NUC-13': 'https://bizweb.dktcdn.net/thumb/1024x1024/100/329/122/products/may-tinh-mini-pc-intel-nuc-13-extreme-kit-i7-13700k-rnuc13rngi70000-6.jpg?v=1680947667597',
  'HP-ENVY-DT': 'https://images-na.ssl-images-amazon.com/images/I/71fOGgAce-L.jpg',
  'DELL-INS-DT': 'https://cdn1615.cdn4s4.io.vn/media/products/may-tinh-de-ban/dell/inspiron/3020mt/inspiron-3020-desktop.webp',
  'ACER-PRE-7K': 'https://cdn.assets.prezly.com/346d8126-820e-4ee0-8b60-a0077acee526/PREDATOR-ORION-7000-PO7-660-02.jpg',
  'NZXT-PL3': 'https://nzxt.com/cdn/shop/files/Player-Three-Prime-ASUS-WW-10.14.25-HERO-WHITE-BADGE_cf7be002-234d-43e0-a2b8-c78f0a7b1844.png?v=1764659862',
  'MAIN-MG1': 'https://cdn.mos.cms.futurecdn.net/HF5NFDnzAF8NDE8znsB5JJ.jpg',
  'ORG-NEURON': 'https://www.originpc.com/blog/wp-content/uploads/2019/12/neuron-hero-red.jpg',
  'ASUS-ROG-DT': 'https://dlcdnwebimgs.asus.com/gain/95E413EB-A725-4131-82B8-FF76A880EE0D',

  // --- TVs ---
  'LG-C3-55': 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/7/1/71557.png',
  'LG-C3-65': 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQWKs4_XrvUIeDRFuognIRCTV-7JIUCPgwRWw&s',
  'LG-G3-65': 'https://cdn.tgdd.vn/Products/Images/1942/306581/smart-tivi-oled-lg-4k-65-inch-65g3psa-1-700x467.jpg',
  'SAM-S90C-55': 'https://images.samsung.com/is/image/samsung/p6pim/ph/qa55s90cagxxp/gallery/ph-oled-s90c-qa55s90cagxxp-536185455',
  'SAM-S90C-65': 'https://images.samsung.com/is/image/samsung/p6pim/ae/qa65s90cauxzn/gallery/ae-oled-tv-qa65s90cauxzn-front-black-titanium-536504295',
  'SAM-QN90C-65': 'https://images.samsung.com/is/image/samsung/p6pim/ae/qa65s90cauxzn/gallery/ae-oled-tv-qa65s90cauxzn-front-black-titanium-536504295',
  'SONY-A80L-55': 'https://bizweb.dktcdn.net/thumb/1024x1024/100/425/687/products/1-a6f4dd7b-9abb-4656-bf44-d498e101dca2.jpg?v=1764054897643',
  'SONY-A95L-65': 'https://logico.com.vn/images/products/2023/03/23/original/a95l-2_1679544846.png',
  'TCL-QM8-65': 'https://i.rtings.com/assets/products/ygyrdRw8/tcl-qm8-qm850g-qled/design-medium.jpg?format=auto',
  'HIS-U8K-65': 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQv6ocmGcD3vR2p-W45Dt85ucV7DBxbalIrJw&s',
  'VIZ-PQ-65': 'https://www.vizio.com/content/dam/vizio/us/en/images/product/2020/tvs/p-series/p65q9-h1/gallery/2020_P-Series_P65Q9-H1_Front_OS_Newsweek-Best-Holiday-Gifts-2020.jpg/_jcr_content/renditions/cq5dam.web.640.480.png',
  'ROKU-PLS-55': 'https://image.roku.com/w/rapid/images/meta-image/51c68c6e-4f37-4204-8bfb-6c9357793922.jpg',
  'AMZ-OMNI-65': 'https://m.media-amazon.com/images/I/61wsF9lZJmL._AC_UF1000,1000_QL80_.jpg',
  'LG-B3-55': 'https://www.lg.com/content/dam/channel/wcms/th/oled-tv/2023/b3-pdp-update/gallery/55-b3-a/TV-OLED-55-B3-A-Gallery-01.jpg/jcr:content/renditions/thum-1600x1062.jpeg',
  'SAM-FRAME-55': 'https://cdnv2.tgdd.vn/mwg-static/dmx/Products/Images/1942/322680/tivi-qled-khung-tranh-samsung-4k-55-inch-qa55ls03d-1-638691037685437659-700x467.jpg',
  'SONY-X90L-65': 'https://sony.scene7.com/is/image/sonyglobalsolutions/TVFY23_X90L_65_12_Beauty_I_M-1?$productIntroPlatemobile$&fmt=png-alpha',
  'TCL-6S-55': 'https://m.media-amazon.com/images/I/91ESqVq-i3L.jpg',
  'HIS-U7K-55': 'https://cdn.nguyenkimmall.com/images/detailed/898/10056785-google-tivi-mini-uled-hisense-4k-55inch-55u7k-1_o1mh-t9.jpg',
  'SAM-CU7-43': 'https://images.samsung.com/is/image/samsung/p6pim/africa_en/ua43cu7000uxly/gallery/africa-en-crystal-uhd-cu7000-ua43cu7000uxly-536771150?$Q90_1248_936_F_PNG$',
  'LG-UR9-50': 'https://m.media-amazon.com/images/I/91MAjR2HydL._AC_UF894,1000_QL80_.jpg',
  'SONY-X80K-43': 'https://cdn.tgdd.vn/Products/Images/1942/274763/android-sony-4k-43-inch-kd-43x80k-180322-024040-550x340.png',
  'INS-F30-50': 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcROQ3i7h3R7eF4Sa0DPzsRNbrnLnJ0CMH0mCQ&s',
  'TOSH-C35-43': 'https://cdn.tgdd.vn/Products/Images/1942/297318/google-tivi-toshiba-4k-43-inch-43c350lp-10-550x340.jpg',
  'LG-C4-55': 'https://thegioithietbiso.com/data/product/rvo1714620647.jpg',
  'SAM-S95D-65': 'https://images.samsung.com/is/image/samsung/p6pim/vn/qa65s95dakxxv/gallery/vn-oled-s95d-qa65s95dakxxv-thumb-540978937',
  'SONY-B9-65': 'https://sony.scene7.com/is/image/sonyglobalsolutions/TVFY24_UP_1_FrontWithStand_M?$productIntroPlatemobile$&fmt=png-alpha',
  'TCL-QM851-75': 'https://sm.pcmag.com/t/pcmag_au/review/t/tcl-qm8-cl/tcl-qm8-class-75-inch-tv-75qm851g_m1j9.1920.jpg'
};

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
    
    // NOTE: Specific imageUrl is handled by the map at the top.
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
        // ==============================================================================
        // LOGIC: Check 'productImages' map first.
        // If not found, use placeholder.
        // ==============================================================================
        const finalImageUrl = productImages[item.sku]
          ? productImages[item.sku]
          : `https://placehold.co/600x400/333333/ffffff/png?text=${encodeURIComponent(item.name)}\n(${item.sku})`;

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
            imageUrl: finalImageUrl,
          },
        });
        allCreatedProducts.push(product);
      }
    }
    console.log(`Created ${allCreatedProducts.length} total products`);

    // 5. Customers
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
      customers.push(await prisma.customer.create({ data: cus }));
    }

    // 6. Orders
    console.log('Seeding orders...');
    if (allCreatedProducts.length > 0 && customers.length > 0) {
      const getRandomProduct = () => allCreatedProducts[Math.floor(Math.random() * allCreatedProducts.length)];
      const getRandomCustomer = () => customers[Math.floor(Math.random() * customers.length)];
      const getRandomUser = () => users[Math.floor(Math.random() * users.length)];
      const getRandomDate = (start: Date, end: Date) => new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));

      const statuses = ['COMPLETED', 'COMPLETED', 'COMPLETED', 'PROCESSING', 'PENDING', 'CANCELLED'];
      const numberOfOrdersToSeed = 100;

      for (let i = 1; i <= numberOfOrdersToSeed; i++) {
        const orderDate = getRandomDate(new Date('2024-01-01'), new Date());
        const orderNum = `ORD-2024-${i.toString().padStart(3, '0')}`;
        const status = statuses[Math.floor(Math.random() * statuses.length)];
        const numItems = Math.floor(Math.random() * 4) + 1;
        const items = [];
        for (let j = 0; j < numItems; j++) {
          const prod = getRandomProduct();
          items.push({ productId: prod.id, quantity: Math.floor(Math.random() * 2) + 1, unitPrice: prod.price });
        }
        const subtotal = items.reduce((sum, item) => sum + Number(item.unitPrice) * item.quantity, 0);

        await prisma.order.create({
          data: {
            orderNumber: orderNum, customerId: getRandomCustomer().id, userId: getRandomUser().id, status: status as any,
            subtotal: new Prisma.Decimal(subtotal), discountAmount: new Prisma.Decimal(0), taxAmount: new Prisma.Decimal(0),
            total: new Prisma.Decimal(subtotal), createdAt: orderDate,
            orderItems: {
              create: items.map(i => ({
                productId: i.productId, quantity: i.quantity, unitPrice: i.unitPrice,
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
        licenseKey: 'MYSHOP-TRIAL-0001', activatedAt: new Date(),
        expiresAt: new Date(new Date().getTime() + 15 * 24 * 60 * 60 * 1000), isActive: true,
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