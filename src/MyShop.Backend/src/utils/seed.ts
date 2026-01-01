import { PrismaClient, Prisma, Product, Customer, Category, Discount } from '@prisma/client';
import { AuthUtils } from './auth';

const prisma = new PrismaClient();

// ==============================================================================
// 1. IMAGE MAPPING (SKU -> URL[])
// ==============================================================================
const productImages: Record<string, string[]> = {
  // --- iPhone 15 Series (1 Original + 2 New) ---
  'IPH-15PM-256': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_3.png',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768365442895_6.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768368578332_2.jpg'
  ],
  'IPH-15PM-512': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_5.png',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768368578332_2.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768365442895_6.jpg'
  ],
  'IPH-15PM-1TB': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_2__5_2_1_1_1_1_2_1_1.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768368578332_2.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768365442895_6.jpg'
  ],
  'IPH-15P-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-plus_1_.png',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768368578332_2.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768365442895_6.jpg'
  ],
  'IPH-15P-256': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_2__5_2_1_1_1_1_2_1_1.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768368578332_2.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2024_4_16_638488768365442895_6.jpg'
  ],
  'IPH-15PL-128': [
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2023_9_13_638302007249847040_iPhone_15_Plus_Blue_Pure_Back_iPhone_15_Plus_Blue_Pure_Front_2up_Screen__USEN.jpg',
    'https://cdn.tgdd.vn/Products/Images/42/303891/iphone-15-plus-1-750x500.jpg',
    'https://cdn.tgdd.vn/Products/Images/42/303891/s16/iphone-15-plus-vang-1-650x650.jpg'
  ],
  'IPH-15PL-256': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone_15/dien-thoai-iphone-15-plus-256gb-3.jpg',
    'https://cdn.tgdd.vn/Products/Images/42/281570/iphone-15-130923-014953.jpg',
    'https://cdn.tgdd.vn/Products/Images/42/303823/iphone-15-plus-256gb-xanh-thumb-600x600.jpg'
  ],
  'IPH-15-128': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/v/n/vn_iphone_15_yellow_pdp_image_position-1a_yellow_color_1_4_1_1.jpg',
    'https://clickbuy.com.vn/uploads/pro/2_51654.jpg',
    'https://cdn.tgdd.vn/Products/Images/42/281570/iphone-15-1-3-750x500.jpg'
  ],
  'IPH-15-256': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone_15/dien-thoai-iphone-15-256gb-8.jpg',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/n/vn_iphone_15_pink_pdp_image_position-9_accessory_1.jpg',
    'https://cdn.tgdd.vn/Products/Images/42/281570/iphone-15-1-3-750x500.jpg'
  ],

  // --- iPhone 14 Series ---
  'IPH-14PM-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/_/t_m_18_1_3_2.png',
    'https://bachlongstore.vn/vnt_upload/product/11_2023/5646.jpg',
    'https://thangtaostore.com/watermark/product/540x540x2/upload/product/14prm-nen-6822.png'
  ],
  'IPH-14P-128': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/v/_/v_ng_12_1_2_1.png',
    'https://bachlongstore.vn/vnt_upload/product/11_2023/5646.jpg',
    'https://thangtaostore.com/watermark/product/540x540x2/upload/product/14prm-nen-6822.png'
  ],
  'IPH-14PL-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/p/h/photo_2022-09-28_21-58-51_4_1_2_2.jpg',
    'https://cdn2.fptshop.com.vn/unsafe/564x0/filters:quality(80)/Uploads/images/2015/Tin-Tuc/02/iPhone-14-Plus.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQkM7pjbtU7CkTq6yJNNpS_VBvfYvEjRwH_JA&s'
  ],
  'IPH-14-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/p/h/photo_2022-09-28_21-58-56_11_1.jpg',
    'https://www.didongmy.com/vnt_upload/product/09_2022/thumbs/(600x600)_14xam_didongmy_600x600.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQkM7pjbtU7CkTq6yJNNpS_VBvfYvEjRwH_JA&s'
  ],

  // --- iPhone 13 & Older ---
  'IPH-13-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-13_2_2.jpg',
    'https://mac24h.vn/images/detailed/92/iPhone13-2021.png',
    'https://trangthienlong.com.vn/wp-content/uploads/2024/11/iphone-13-thuong-vs-iphone-13-mini-128gb-256gb-512gb.jpg'
  ],
  'IPH-13M-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/1/4/14_1_9_2_6.jpg',
    'https://cdsassets.apple.com/live/SZLF0YNV/images/sp/111872_iphone13-mini-colors-480.png',
    'https://cdn.tgdd.vn/Products/Images/42/236780/Kit/iphone-13-mini-n.jpg'
  ],
  'IPH-12-64': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-12.png',
    'https://trangthienlong.com.vn/wp-content/uploads/2024/11/iphone-12-thuong-vs-iphone-12-mini-64gb-128gb-256gb.jpg',
    'https://bvtmobile.com/uploads/source/iphone12-1/iphone-12-purple.jpg'
  ],
  'IPH-SE3-64': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/1/_/1_359_1.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT8NgCLku1R6mXVtCPnfcJ69eCVlKUTDw2F9A&s',
    'https://cdn.tgdd.vn/Files/2022/01/18/1411437/265266695_455337209385670_598702_1280x1596-800-resize.jpg'
  ],
  'IPH-SE3-128': [
    'https://cdn2.fptshop.com.vn/unsafe/828x0/filters:format(webp):quality(75)/2022_4_15_637856361035158510_iPhone%20SE%20(8).jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT8NgCLku1R6mXVtCPnfcJ69eCVlKUTDw2F9A&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTG4irNnFC0x8E0XZWuOjNEhbuP4f3gEKF75g&s'
  ],
  'IPH-11-64': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-11.png',
    'https://product.hstatic.net/200000768357/product/_thuong__-_color_321d25895d074fcb834639ed7bd57c89.png',
    'https://cdn.hstatic.net/products/1000359786/dsc04242_b7d12b5a18804611994a2973ddcc37da_master.jpg'
  ],

  // --- Used / Refurbished ---
  'IPH-12P-REF': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/d/o/download_4_2_2.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRTORePfP_KQXTOlzpv1KwOIkGftpyXwe-kfw&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTfGa9_vOfQFnIp4khLHO_CUX7J6kqOJQ3N_Q&s'
  ],
  'IPH-12PM-REF': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/d/o/download_2__1_27.png',
    'https://qkm.vn/wp-content/uploads/2024/07/iphone-12-pro-128gb-256gb-512gb-cu-like-new-9-qkm-1.jpg',
    'https://images.tokopedia.net/img/cache/700/VqbcmM/2024/3/26/15fe9df0-1b80-491f-8095-df4973d45416.jpg'
  ],
  'IPH-XSM-USED': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone_xs_max_512gb_1_1.jpg',
    'https://bizweb.dktcdn.net/thumb/grande/100/372/421/products/apple-iphone-xs-black-92641fd4-2491-46ce-8443-6ec1a6b50b74.png?v=1741951045827',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS9LyRziohMSb_26zq7VG6IxCg1Br0-tW-R8g&s'
  ],
  'IPH-XR-USED': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone_xr_64gb_1.png',
    'https://truonggiang.vn/wp-content/uploads/2022/05/iphone-xr-64gb-cu-1.jpg',
    'https://cdn.tgdd.vn/Products/Images/42/230406/Kit/iphone-xr-64gb-hop-moi-note-1.jpg'
  ],
  'IPH-8P-USED': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone8-plus-silver-select-2018_6_3.png',
    'https://truonggiang.vn/wp-content/uploads/2021/02/iphone-8-plus-64gb-2.jpg',
    'https://24hstore.vn/images/products/2025/05/30/large/iphone-8-plus-64gb-cu-98.jpg'
  ],

  // --- iPhone 16 Series ---
  'IPH-16PM-256': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/p/h/photo_2024-10-02_13-59-00_1.jpg',
    'https://rauvang.com/data/Product/iphone-16-pro-max-all.htm_1726020874.jpg',
    'https://hdmobi.vn/wp-content/uploads/2024/11/iphone-16-pro-max-400x400.jpg'
  ],
  'IPH-16P-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-pro_1.png',
    'https://rauvang.com/data/Product/iphone-16-pro-max-all.htm_1726020874.jpg',
    'https://hdmobi.vn/wp-content/uploads/2024/11/iphone-16-pro-max-400x400.jpg'
  ],
  'IPH-16PL-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-plus-1.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-plus-6.png',
    'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/42/329138/iphone-16-plus-1-638639830699738117.jpg'
  ],
  'IPH-16-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16-1.png',
    'https://bvtmobile.com/uploads/source/ip16/3ead4148a56b2b136ab7581af5df98af.jpg',
    'https://www.didongmy.com/vnt_upload/product/09_2024/thumbs/(600x600)_iphone_16_mau_trang_didongmy_thumb_600x600.jpg'
  ],
  'IPH-16E-128': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-16e-128gb.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQV6a2rHtKofwe4LaZwa9tmLrKhAGQiczbBKQ&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQIR0kuICiJMkpgh7VxokXayX3Jx9WBY5yneg&s'
  ],

  // --- iPads (Updated) ---
  'IPD-PRO12-M2-128': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-pro-13-select-wifi-spacegray-202210-02_3_3_1_1_1_4.jpg',
    'https://hoangsonstore.com/wp-content/uploads/2023/01/ipad-pro-m2-12-9-inch-2022-wifi-128gb-moi-100-4498-9.jpg',
    'https://phucanhcdn.com/media/product/49294_wifi_128gbb.jpg'
  ],
  'IPD-PRO12-M2-256': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-13-select-202210_3_1.png',
    'https://cdn.tgdd.vn/Products/Images/522/295468/Slider/ipad-pro-m2-12.9-inch-wifi-cellular-256gb638030923425442302.jpg',
    'https://product.hstatic.net/200000525189/product/ipad_pro_xam_1f32e1ae3df44c35b5b3b65cbbae9c94_1024x1024.png'
  ],
  'IPD-PRO12-M2-512': [
    'https://cdn.tgdd.vn/Products/Images/522/295464/ipad-pro-m2-12.5-wifi-xam-thumb-600x600.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTm81Tp_UUt-55DSUzXRuHPEqye7el7SxUsvQ&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRR4WRkax6esH-dLzqJX2NxCCOKUQ5nUEFswA&s'
  ],
  'IPD-PRO11-M2-128': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-pro-13-select-wifi-silver-202210-01_4.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSIWAz27a_a3nqmpNV0twAHy7e5zIM7whm0ww&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRR4WRkax6esH-dLzqJX2NxCCOKUQ5nUEFswA&s'
  ],
  'IPD-PRO11-M2-256': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-13-select-202210_1_1_1.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQDst8InmVt2Cv-8_KE-jlbsch99V6JBWho0w&s',
    'https://product.hstatic.net/1000329106/product/ipad-pro-m2-wifi-bac-4_c431a0f9999a4c1e84f87eb9efd385fa_master.jpg'
  ],
  'IPD-AIR5-64': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-air-5.png',
    'https://bachlongstore.vn/vnt_upload/product/11_2023/thumbs/1000_43543.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTe11ZOYkPc9x8kxEZXwptVw50MYraBpZ1kRA&s'
  ],
  'IPD-AIR5-256': [
    'https://cdn.tgdd.vn/Products/Images/522/274154/ipad-air-5-wifi-blue-thumb-1-600x600.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR0iDUvTzRZcx7B5tD4_3WNsHpgAGMemqYKFw&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTTbOsDPRvn7Cjx-7BxOQjqqspjyvdpAwwihA&s'
  ],
  'IPD-MINI6-64': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/_/t_i_xu_ng_2__1_8_1_1.png',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-mini-6-5.jpg',
    'https://cdn.tgdd.vn/Products/Images/522/248091/ipad-mini-6-13.jpg'
  ],
  'IPD-MINI6-256': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-mini-6-5_1_1_1_1.jpg',
    'https://minhdatstore.vn/public/uploads/ipad-mini-6-glr-1.jpg',
    'https://laptop360.net/wp-content/uploads/2023/04/Apple-iPad-Mini-6-4.jpg'
  ],
  'IPD-10-64': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-10-9-inch-2022.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/tablet/iPad/iPad-gen-10/ipad-10-9-inch-2022-7.jpg',
    'https://mac24h.vn/images/detailed/92/IPAD_GEN_10_MAC24H.png'
  ],
  'IPD-10-256': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-2022-hero-blue-wifi-select_1.png',
    'https://mac24h.vn/images/detailed/92/IPAD_GEN_10_MAC24H.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSzjN61i55zCHHpH35T7mOVY37YdkPQsSHkUA&s'
  ],
  'IPD-9-64': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/2/c/2c_v.png',
    'https://cdn.tgdd.vn/Products/Images/522/247517/ipad-gen-9-2.jpg',
    'https://phucanhcdn.com/media/lib/09-10-2021/ipadgen9102bvh8.jpg'
  ],
  'IPD-9-256': [
    'https://bizweb.dktcdn.net/thumb/1024x1024/100/401/951/products/dacdiemnoibatad7358efe2ed47aa9-6fd11bbc-2a77-4216-b94b-08369a6a8e34.png?v=1749147182043',
    'https://phucanhcdn.com/media/lib/09-10-2021/ipadgen9102bvh8.jpg',
    'https://www.civip.com.vn/media/product/10281_10050587_ipad_gen_9_wifi_256gb_10_2_inch_mk2p3za_a_bac_2021_4.jpg'
  ],
  'IPD-PRO12-M1-REF': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-pro-12-9-2021_1_1_2_1_1_2.jpg',
    'https://dienthoaigiakho.vn/_next/image?url=https%3A%2F%2Fcdn.dienthoaigiakho.vn%2Fphotos%2F1678077654796-ipad-12.9-2021-128-1.jpg&w=3840&q=75',
    'https://cdn.viettablet.com/images/companies/1/0-hinh-moi/tin-tuc/2021/thang-1/13-1/ipad-pro-2021.jpg?1610621599027'
  ],
  'IPD-PRO11-M1-REF': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-11-2021-2_1_1_1_1_1_1_1_1_1_1_1_1_1_1.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT3vHN-tGsrVWq25QaavkiD9SDzHRK2gQyxfg&s',
    'https://product.hstatic.net/200000373523/product/-tinh-bang-ipad-pro-m1-2021-11-inch-wifi-8gb-128gb-mhqr3za-a-xam-01_1__a02b1bc769cc4ccd85faee6a5820b984_grande.jpg'
  ],
  'IPD-AIR4-REF': [
    'https://ttcenter.com.vn/uploads/product/zsr37jz9-364-ipad-air-4-10-9-inch-wifi-64gb-like-new.jpg',
    'https://assets.kogan.com/images/brus-australia/BRS-APPLE-IPAD-AIR-4-64GB-W-ANY-G/1-90785cef98-apple_ipad_air_4_any_colour_new.jpg?auto=webp&bg-color=fff&canvas=1200%2C800&dpr=1&enable=upscale&fit=bounds&height=800&quality=90&width=1200',
    'https://cdn.viettablet.com/images/companies/1/0-hinh-moi/tin-tuc/2020/17-9/ipad-air-4-mau-sac.jpg?1600340943574'
  ],
  'IPD-MINI5-USED': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-mini-select-wifi-silver-201903_7_1.png',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad-mini-5_3_1.jpg',
    'https://macvn.com.vn/wp-content/uploads/2023/10/ipad-mini-gen-5-1.jpg'
  ],
  'IPD-PRO12-2020': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/a/p/apple-ipad-pro-11-2020-wifi-256-gb-2_2_1_1.jpg',
    'https://2tmobile.com/wp-content/uploads/2022/07/ipad_pro_12_9_inch_2021_2tmobile.jpg',
    'https://cdn.tgdd.vn/Products/Images/522/221775/ipad-pro-12-9-inch-wifi-128gb-2020-8.jpg'
  ],
  'IPD-PRO11-2020': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/i/p/ipad_pro_11_2020_bac_3_1_2.jpg',
    'https://cdn.tgdd.vn/Products/Images/522/220163/Slider/ipad-pro-11-inch-2020-073220-033200-788.jpg',
    'https://2tmobile.com/wp-content/uploads/2022/10/ipad-pro-11-inch-m2-series.jpg'
  ],
  'IPD-8-USED': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/1/9/19268_ipadgen8sliver_ok_3.jpg',
    'https://bizweb.dktcdn.net/thumb/grande/100/401/951/products/ipad-gen-8-2.png?v=1730041219287',
    'https://i.ebayimg.com/images/g/caIAAOSw5DpgOTCx/s-l400.jpg'
  ],
  'IPD-AIR3-USED': [
    'https://hoangtrungmobile.vn/wp-content/uploads/2021/03/ipad-air-3-bac.jpg',
    'https://hoangtrungmobile.vn/wp-content/uploads/2021/03/ipad-air-3-1.png',
    'https://cellphones.com.vn/media/wysiwyg/tablet/apple/apple-ipad-air-105-wifi-64gb-chinh-hang-2.jpg'
  ],
  'IPD-PRO105-USED': [
    'https://didongthongminh.vn/images/products/2025/09/19/original/2(4).jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRpVa9xWhpe-lggq2UlEEtghDF-OS1Qf3udKA&s',
    'https://2tmobile.com/wp-content/uploads/2022/10/ipad-pro-10-5-2017-rose-gold.jpg'
  ],
  'IPD-PRO97-USED': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-pro-9in-gold_3_1_2_1.jpg',
    'https://24hstore.vn/images/products/2024/10/30/large/ipad-pro-9-7-2016-wifi-cellular-cu.jpg',
    'https://shopdunk.com/images/thumbs/0016536_DSC06746-1-800x450_1600.jpeg'
  ],

  // --- Laptops (Updated) ---
  'MAC-AIR-M3-13': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/m/b/mba13-m3-midnight-gallery1-202402_3_1_2_1.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ9ciFFwk8JAZzSj_HnhFmw7R9V0VFcUET99w&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRzcw-AOkamn41r6w98MjF_GgPYGMBN2k7XiQ&s'
  ],
  'MAC-AIR-M3-15': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/laptop/macbook/Air/M3-2024/macbook-air-m3-15-inch-2024-1_1.jpg',
    'https://cdn.tgdd.vn/News/1562806/1-1280x720.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRfgklLIT3u0jp1zknXIoVcOYGWCfXck8Qk6A&s'
  ],
  'MAC-PRO-14-M3': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/laptop/macbook/macbook-pro-7.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS0LaLe2Dxa6uVFmI1mmdTO4EkcbnYa1Jq7aQ&s',
    'https://helios-i.mashable.com/imagery/reviews/04xYfyZ1mk1LcppkgmbOzG5/images-14.fill.size_2000x1125.v1701668675.jpg'
  ],
  'MAC-PRO-16-M3P': [
    'https://bizweb.dktcdn.net/100/318/659/products/7-5581399e-3267-456a-8d4f-9259ac8f5dc0.jpg?v=1699430082770',
    'https://macone.vn/wp-content/uploads/2023/12/apple-macbook-pro-2023-4.jpeg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS28xgUz1fbA0-xerg3SyMgoIvava19dSAn9w&s'
  ],
  'MAC-AIR-M2-13': [
    'https://cdn2.fptshop.com.vn/unsafe/macbook_air_13_m2_midnight_1_35053fbcf9.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTHPmPFYftfJXaQvAPdwjZDxLfusqCZpVJW9g&s',
    'https://shopdunk.com/images/thumbs/0018828_so-sanh-macbook-air-m2-13-inch-va-15-inch_1600.jpeg'
  ],
  'DELL-XPS-13P': [
    'https://cellphones.com.vn/sforum/wp-content/uploads/2022/01/tren-tay-Dell-XPS-13-Plus-13.jpg',
    'https://cellphones.com.vn/sforum/wp-content/uploads/2022/01/tren-tay-Dell-XPS-13-Plus-12.jpg',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/t/e/text_ng_n_3__7_102.png'
  ],
  'DELL-XPS-15': [
    'https://media.wired.com/photos/6169f03b58660fcbc5f4ec3b/master/w_1600%2Cc_limit/Gear-Dell-XPS-15-OLED-1.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTSwRbnCnclo9IzxAzIdB1v2r2-jPIVF7DRSw&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ5JJ7qgZP-lZq8X05QvJEObTdoWK7g6OrFaw&s'
  ],
  'LEN-X1-G11': [
    'https://cdn-media.sforum.vn/storage/app/media/wp-content/uploads/2023/11/10-6.jpg',
    'https://www.laptopvip.vn/images/companies/1/JVS/LENOVO/TP-X1C-G11/10006.jpg?1680061438462',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRltvy2CyXoprbFFNwIYQ22jFcoklCJaBdv1w&s'
  ],
  'HP-SPEC-14': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/l/a/laptop-hp-spectre-x360-14-ea0023xd-cu-dep-3_1.jpg',
    'https://khoaquan.vn/wp-content/uploads/2023/12/803M6EA-ABU_14_1750x1285.webp',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQJsFo4sNurYpNvb2Xlkmm7E13Dc4GeHrHU3g&s'
  ],
  'ASUS-ZEN-14': [
    'https://nguyencongpc.vn/media/product/20151-asus-zenbook-duo-14-ux482eg-ka166t-6.jpg',
    'https://dlcdnwebimgs.asus.com/gain/e1c062dc-b3ad-4b84-b310-eda9f5984d2c/',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSBsXHWAG648DddEZ-ShffoL3w3vx3FcB1uTQ&s'
  ],
  'RAZ-BLD-14': [
    'https://laptops.vn/wp-content/uploads/2024/06/razer-blade-14-2024-1710487813_1711595620-1.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRHrfvgh-p4VzqO_ukL1RFy_dSL7VTITruJ3Q&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS4IlJyBL2ICq8x16wOPdvtUxMrka3nMeBYpA&s'
  ],
  'MS-SURF-L5': [
    'https://vhost53003.vhostcdn.com/wp-content/uploads/2022/10/microsoft-surface-laptop-5-2.jpg',
    'https://cdn-media.sforum.vn/storage/app/media/wp-content/uploads/2022/11/Microsoft-Surface-Laptop-5-20.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS7MkaUHUjsps5gFKJWbC8MuinVcQSPybyRbw&s'
  ],
  'LG-GRAM-17': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/e/text_ng_n_55__2_11.png',
    'https://mac24h.vn/images/detailed/94/LG_GRAM_17.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQr4OucSL4UoxtCNtv_h8eiJF8W28_cAS_DVw&s'
  ],
  'SAM-BOOK3-PRO': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/3/6/360pro_1.png',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/g/b/gb3_pro-2-configurator-800x600_1_2.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcScwPLewEHjR5BC8ANjydiYyMSEC3JMLrQLZA&s'
  ],
  'ACER-SWF-5': [
    'https://cellphones.com.vn/sforum/wp-content/uploads/2020/06/Acer-Swift-5-SF514-55-Standard_01.png',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/l/a/laptop-acer-swift-5-sf514-55t-51nz-2.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQYQ869QAlPAQX_FyX8HJfDNMUiKfvVrSmdog&s'
  ],
  'MSI-ST-16': [
    'https://product.hstatic.net/200000722513/product/057vn_9630c86ceec944c49425ef01bb5c879d_master.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ_mR-FETn-GwuAM8dMT0RpjJLHkBORjMZcwg&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQfqk2m7tRGH-ImM1uqXkrG4bkilwjxetbDdg&s'
  ],
  'LEN-YOGA-9I': [
    'https://cdn-media.sforum.vn/storage/app/media/chidung/yoga-9i/danh-gia-yoga-9i-2024-14.jpg',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/e/text_ng_n_19__4_11.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ1qo2a1bop9KKSVMh8FIwH6HC4e-Mbu5HAaA&s'
  ],
  'ALN-X14': [
    'https://www.laptopvip.vn/images/ab__webp/detailed/32/notebook-alienware-x14-r2-gray-gallery-6-www.laptopvip.vn-1686985486.webp',
    'https://laptop15.vn/wp-content/uploads/2023/08/Dell-Alienware-X14-R1-2.png',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/l/a/laptop-alienware-x14-r1-3.png'
  ],
  'MAC-PRO13-M2-REF': [
    'https://product.hstatic.net/200000768357/product/gray_fab6e9b0c7374bfd86b9189632447680.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRAYJ8TYeoCN6rNc9ymhjhgw46k7_Aj3aDwqQ&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTiVkQY_qplE2tLMlAPIfS3wTuy_rT2L-5xfA&s'
  ],
  'DELL-INS-15': [
    'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/330075/dell-inspiron-15-3520-i5-n3520-i5u085w11slu-1-638627942653445825-750x500.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRYIV3NYrS7zeOBD7mGLu5305ky5Ss45qjlvg&s',
    'https://www.laptopvip.vn/images/ab__webp/detailed/31/ava-x8q4-0b-www.laptopvip.vn-1678677543.webp'
  ],
  'HP-PAV-15': [
    'https://www.laptopvip.vn/images/companies/1/JVS/HP/HP-Pavilion-15T/71lc66S1jqL._AC_SL1500_.jpg?1666681642506',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRdAuOwmtb2G1uy9U7DDpZk2IhAhisOZq-dvw&s',
    'https://laptopbaoloc.vn/wp-content/uploads/2023/02/Laptop-HP-Pavilion-15-eg0507TU-3.jpg'
  ],
  'ASUS-VIVO-15': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/e/text_ng_n_1__5_16_1.png',
    'https://bizweb.dktcdn.net/thumb/1024x1024/100/329/122/products/laptop-asus-vivobook-15-x1504va-nj070w-4.jpg?v=1696089769407',
    'https://vt.net.vn/wp-content/uploads/2020/10/asus-vivo15-a1505va-l1114w-1.jpg'
  ],
  'LEN-IDEA-3': [
    'https://p3-ofp.static.pub/fes/cms/2022/12/28/cbsimp9kdhc8w1tw2t7pytz6exsvvv545729.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTDgTiw21igwZMsUXiUsbcZHdXNC-oDr1f0HA&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSL2uB2sPHgWesRdvJPxCnqB6JEdtLPEYfKaQ&s'
  ],
  'MAC-PRO-14-M4': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/laptop/macbook/macbook-pro-7.jpg',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/m/a/macbook_pro_14_inch_m4_chip_silver_pdp_image_position_2_vn_vi.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSObbZAo9rBWO4d3gnK7bE3eIrk-FoddFLkSg&s'
  ],
  'DELL-XPS-14-9440': [
    'https://hungphatlaptop.com/wp-content/uploads/2024/01/DELL-XPS-14-9440-2024-Platinum-H1-1.jpeg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTK253slDwNKaR1CF_P5Y07C2LOLhhAI8m8GA&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSrx-QJYF4Rg8UE2-dviGO0MfhdAbq0gj7BlQ&s'
  ],
  'HP-OMNI-FLIP': [
    'https://cdn2.fptshop.com.vn/unsafe/800x0/hp_omnibook_ultra_flip_14_fh0040tu_b13vhpa_6_2a358fe388.jpg',
    'https://2tmobile.com/wp-content/uploads/2025/05/hp-omnibook-ultra-flip-2024-2tmobile.webp',
    'https://www.pcworld.com/wp-content/uploads/2025/04/HP-OmiBook-Ultra-Flip-14-tablet-mode.jpg?quality=50&strip=all&w=1024'
  ],
  'ASUS-ROG-G14-24': [
    'https://dlcdnwebimgs.asus.com/gain/E90DE227-7002-48C1-A940-B6E952D0BCCC',
    'https://lapvip.vn/upload/products/original/asus-gaming-rog-zephyrus-g14-windows-10-1597659277.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS6WCnBRFZF-mVz8vdU7DUAbkcm52uPiyE4rw&s'
  ],
  'MS-SURF-L7': [
    'https://surfaceviet.vn/wp-content/uploads/2024/05/Surface-Laptop-7-Black-15-inch.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTm-PQBOqfnRaqtr0G46MBSI-lYIkLxQNK5xA&s',
    'https://hips.hearstapps.com/vader-prod.s3.amazonaws.com/1724683162-1722447273-microsoft-surface-laptop-2024-001-66aa759807ea4.jpg?crop=0.712xw:0.949xh;0.106xw,0.0342xh&resize=980:*'
  ],

  // --- Tablets (Updated) ---
  'SAM-S9-ULT': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tab-s9-ultra-kem-2_1_1.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRbLFI74_smEYhr-YQcuYCQWT-2CAGcqnqrCA&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQG5-j-Zk08C5eKFICM-FhrAFJjsH0qtbip-w&s'
  ],
  'SAM-S9-PLS': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/e/p/eprice_1_b7620c148ab010a64546e96a356978b2_2_1.jpg',
    'https://cdn.tgdd.vn/Products/Images/522/307178/Slider/samsung-galaxy-tab-s9-plus-thumb-yt-1020x570.jpg',
    'https://cdn-v2.didongviet.vn/files/default/2024/11/17/0/1734426091261_2_samsung_galaxy_tab_s9_plus_256gb_didongviet.jpg'
  ],
  'SAM-S9': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/s/ss-tab-s9_1.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSWIurcQN_FHag_Kc3f4tbLSflbL_LceBKxRg&s',
    'https://bachlongstore.vn/vnt_upload/product/10_2023/thumbs/1000_732176.png'
  ],
  'SAM-S9-FE': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tab-s9-fe-xam_2_1_1.png',
    'https://happyphone.vn/wp-content/uploads/2024/03/Samsung-Galaxy-Tab-S9-FE-Wifi-128GB-Xanh-mint.png',
    'https://cdn.tgdd.vn/Products/Images/522/309819/Slider/samsung-galaxy-tab-s9-fe-plus-tongquan-1020x570.jpg'
  ],
  'GOO-PIX-TAB': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/g/o/google_pixel_tablet.jpg',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/tablet/Google/google-pixel-tablet-4.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTz6SEJwyMo1YdZ3nlSMHgojasS7pnR-LG3QQ&s'
  ],
  'ONE-PAD': [
    'https://cdn.viettablet.com/images/thumbnails/480/516/detailed/56/oneplus-pad-chinh-hang.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSinNk73b8swO4DX16Le9UCXMx59RUXThZfSw&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWiCI9Q92F0PDblHbMwoKORXuRKgpQaUFj-Q&s'
  ],
  'LEN-P12-PRO': [
    'https://p4-ofp.static.pub/fes/cms/2023/03/28/7dch8vg9lz0tzeg74u3x9paoln4o8z319478.png',
    'https://cellphones.com.vn/sforum/wp-content/uploads/2023/07/Lenovo-Tab-P12-3.jpeg',
    'https://p2-ofp.static.pub/fes/cms/2023/03/28/8wauc5kf0e16qej7g7cmhrspz7e2ov405745.jpg'
  ],
  'XIA-PAD-6': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/i/mi-pad-6-cps-doc-quyen-xanh_3_1_1.jpg',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/x/i/xiaomi_pad6_-_1.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQBIB74sMizNbFsFBamVCKLmRUXZA8JdLiZ6A&s'
  ],
  'AMZ-FIRE-11': [
    'https://product.hstatic.net/200000730863/product/51gj5oqxbnl._ac_sl1000__f75a5ec479ef4fc1ac2f78b17c6da98d_master.jpg',
    'https://kindlehanoi.vn/wp-content/uploads/2023/08/Fire-Max-11-2023-2024-model.jpg',
    'https://m.media-amazon.com/images/I/51-hmSQ2FsL._UF1000,1000_QL80_.jpg'
  ],
  'SAM-A9-PLS': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-tab-a9_11__1.png',
    'https://cdn.tgdd.vn/Products/Images/522/315590/Slider/samsung-galaxy-tab-a9-plus-thumb-1020x570.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTUJm3ql3pJ6k2YlTw1nXXTTddlDg7O13hSeg&s'
  ],
  'LEN-M10-PLS': [
    'https://p2-ofp.static.pub/fes/cms/2023/03/29/8rz4mn5wzzx3s29zfcffctkb2xcwjj602719.png',
    'https://cdn.tgdd.vn/Files/2021/03/19/1336509/lenovotabm10hd-2_800x450.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSc1qdbZMQHw9FzagItRiCuGjKEzl2vB-U9og&s'
  ],
  'SAM-S8-ULT-REF': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/h/t/https___bucketeer_e05bbc84_baa3_437e_9518_adb32be77984.s3.amazonaws.com_public_images_b08df22d_4b5e_46a8_87c5_fc303e133f8a_1500x1500_1_1_1_1.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR06MlEDQr-ItwYbVMhDASbC0FAs_IefMXuQA&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTnhyMRMNvOEwWJPqlO-DWpVHGzV6FVbF2-sA&s'
  ],
  'SAM-S8-PLS-REF': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/e/series_tab_s8001_1_2.jpg',
    'https://files.refurbed.com/ii/samsung-galaxy-tab-s8-plus-1666339074.jpg?t=fitdesign&h=600&w=800',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTrc9d2K5eYzLkn9H-xzFjZ2Pa4TJKGdweXCw&s'
  ],
  'MS-SURF-P9': [
    'https://surfacecity.vn/wp-content/uploads/microsoft-surface-pro-9-5g.jpg',
    'https://surfacestore.com.vn/wp-content/uploads/2022/10/microsoft-surface-pro-9-1.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTGG1Do0cRbDI-6IAY7-bbaq7FeA_FmoJzCIg&s'
  ],
  'MS-SURF-G3': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/5/6/5650372_surface_go_3_under_embargo_until_22.jpg',
    'https://hanoilab.com/wp-content/uploads/2024/10/Surface-Laptop-Go-3-New-Open-Box-Ha-Noi-Lab-5.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSJCJshmeKQ9OJMsAyClsHXuYV2ic-piYrhuQ&s'
  ],
  'CHU-HIPAD': [
    'https://www.chuwi.com/public/upload/image/20221229/52bc2a3d58a50a2bb14171419cc30094.png',
    'https://dt24h.com/wp-content/uploads/2023/09/CHUWI-HiPad-XPro-15.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQxXrVcWEH0zMNdKDHucFk1c8nRWZCul9iczQ&s'
  ],
  'TEC-T50': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/may-tinh-bang-teclast-t50-plus_1_.png',
    'https://cdn-media.sforum.vn/storage/app/media/ace%20chu%20tu/tren-tay-teclast-t50-plus/ava.jpg',
    'https://en.teclast.com/cdn/shop/files/10_acd659ea-17cb-4b0f-9626-31788d36421b.jpg?v=1686395318&width=1000'
  ],
  'NOK-T21': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/n/o/nokia-t21_12_.png',
    'https://cdn.tgdd.vn/Files/2022/09/01/1461522/nokia-t21_1280x720-800-resize.jpg',
    'https://cdn.tgdd.vn/Files/2023/03/21/1519337/a2-210323-065518-800-resize.jpg'
  ],
  'REA-PAD-2': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/r/e/realme-pad-2.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRqxAYc62sIRw4x-H53FnJVbOo3suSyy4p22w&s',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRlPuADAMojDUV_RmBf3lBhZtraWG6z1NgXuA&s'
  ],
  'OPP-PAD-A': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/o/p/oppo-pad-air-128gb.jpg',
    'https://cdn.tgdd.vn/Products/Images/522/281821/Slider/oppo-pad-air-thumb-YT-1020x570.jpg',
    'https://cdn.tgdd.vn/Products/Images/522/281821/oppo-pad-air-1-1-750x500.jpg'
  ],
  'VIV-PAD-2': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/v/i/vivo-pad-2_2_.jpg',
    'https://cdn.tgdd.vn//News/0//vivo-pad-2-ra-mat-lo-dien-3-730x408.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkgzolOndfQzUnYtzWEdLdKdX6llp48SSTyQ&s'
  ],
  'HUA-MATE-13': [
    'https://cellphones.com.vn/sforum/wp-content/uploads/2023/10/Huawei-Matepad-Pro-13.9-3.jpg',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/3/_/3_48_20.jpg',
    'https://cdn.tgdd.vn/News/1558700/10-1280x720.jpg'
  ],
  'HON-PAD-9': [
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/m/a/may-tinh-bang-honor-pad-9-pro_3_.png',
    'https://cdn.tgdd.vn/News/0/d-1280x720.jpg',
    'https://www.notebookcheck.net/fileadmin/_processed_/a/a/csm_20240315_144254_5048161306.jpg'
  ],

  // --- PCs (Updated) ---
  'MAC-MINI-M2': [
    'https://mac24h.vn/images/companies/1/12inch%20rose/Macbook%2012%20inch%20gold/macminipost.png?1598064081104',
    'https://macone.vn/wp-content/uploads/2023/01/m2-mac-mini-copy.jpg',
    'https://hoanghamobile.com/tin-tuc/wp-content/uploads/2023/03/Mac-mini-2023-2.jpg'
  ],
  'MAC-MINI-M2P': [
    'https://cdn2.cellphones.com.vn/200x/media/catalog/product/m/a/macbook_33_.png',
    'https://macone.vn/wp-content/uploads/2023/02/mac-mini-m2-pro.png',
    'https://cdn.tgdd.vn/Files/2023/01/20/1504319/macminitrolaimanhmehonkhiduoctrangbichipapplem2pro_1280x720-800-resize.jpg'
  ],
  'MAC-STUDIO-M2': [
    'https://product.hstatic.net/200000348419/product/mac_studio_m2_max_2023_chinh_hang_21aed22940d54b5f8c6bc1e92f721ab1_large.png',
    'https://shopdunk.com/images/thumbs/0018104_mac-studio-m2-max.jpeg',
    'https://laptop15.vn/wp-content/uploads/2023/08/Mac-Studio-M1-Max-1.png'
  ],
  'MAC-STUDIO-ULT': [
    'https://macstores.vn/wp-content/uploads/2023/06/mac-studio-m2-4.jpg',
    'https://macmall.vn/uploads/screen_shot_2024-06-05_at_16_1717580424.29.17.png',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQHReyPkahn7iL_EZFqHxA9DqTBiow747DXHg&s'
  ],
  'IMAC-24-M3': [
    'https://shopdunk.com/images/thumbs/0022756_imac-m3-2023-24-inch-8-core-gpu8gb256gb.jpeg',
    'https://www.apple.com/newsroom/images/2023/10/apple-supercharges-24-inch-imac-with-new-m3-chip/article/Apple-iMac-M3-colors-231030_big.jpg.large.jpg',
    'https://mac24h.vn/images/companies/1/phu%CC%A3%20kie%CC%A3%CC%82n/Hyperdrive/imac%2024%20inch/IMAC%2024%20INCH.jpg?1718860710656'
  ],
  'DELL-XPS-DT': [
    'https://www.laptopvip.vn/images/ab__webp/detailed/10/dell-xps-27.webp',
    'https://khoavang.vn/resources/cache/800xx1/data/dell/DEll-G5-5090/Dell-XPS-8940--4-1633518499.webp',
    'https://i.pcmag.com/imagery/reviews/061gj28ssJiSsCmgbTfUojq-6.fit_lim.size_1050x.jpg'
  ],
  'ALN-AUR-R16': [
    'https://images-na.ssl-images-amazon.com/images/I/71C+ewM2JjL.jpg',
    'https://sm.pcmag.com/pcmag_me/photo/default/078fwui2ishfdtodnicyblr-6_353v.jpg',
    'https://images-na.ssl-images-amazon.com/images/I/61rLBkZzFJL.jpg'
  ],
  'HP-OMEN-45': [
    'https://kaas.hpcloud.hp.com/PROD/v2/renderbinary/7477130/5038347/con-win-dt-p-omen-45l-gt22-1009nf-product-specifications/articuno-desktop',
    'https://www.tnc.com.vn/uploads/news/20220729/omen-45l-may-bo-hp-gaming-de-dang-nang-cap%202.png',
    'https://cdn.mos.cms.futurecdn.net/G9H9NhYMwFL3pszTEJrctG.jpg'
  ],
  'LEN-LEG-T7': [
    'https://p2-ofp.static.pub//fes/cms/2024/11/27/em8bpvjffmescc7mck5snjp1g73otp127407.png',
    'https://p4-ofp.static.pub//fes/cms/2024/02/29/pcuk3jaes0a1yst8mollyq0nt8fn2c820605.jpg',
    'https://p4-ofp.static.pub//fes/cms/2025/03/28/fxl0ptkilucyo10w6zpmy4ev6cv32h744429.jpg'
  ],
  'COR-VEN-I7': [
    'https://res.cloudinary.com/corsair-pwa/image/upload/v1684950787/products/Vengeance-PC/CS-9050047-NA/Gallery/common/Vengeance__i7400_01.webp',
    'https://thegadgetflow.com/wp-content/uploads/2023/03/CORSAIR-Vengeance-i7400-Frost-Edition-Gaming-PC-01.jpg',
    'https://assets.corsair.com/image/upload/c_pad,q_85,h_1100,w_1100,f_auto/products/Vengeance-PC/CS-9050062-NA/Vengeance_i7400_Frost_Edition_01.webp'
  ],
  'MSI-AEGIS': [
    'https://asset.msi.com/resize/image/global/product/product_1669160633809914962a2cb40d02df74877b17555b.png62405b38c58fe0f07fcef2367d8a9ba1/1024.png',
    'https://i.pcmag.com/imagery/reviews/032R5yzbNKqbXJAXNujVkBu-2..v1616773109.jpg',
    'https://asset.msi.com/resize/image/global/product/product_164989404970502cdd1b0b28ad95c2afe110e916bf.png62405b38c58fe0f07fcef2367d8a9ba1/600.png'
  ],
  'SKY-AZU-GM': [
    'https://m.media-amazon.com/images/I/71gtoidr0kL._AC_UF894,1000_QL80_.jpg',
    'https://i.ytimg.com/vi/KFxk20EoSpc/hq720.jpg?sqp=-oaymwEhCK4FEIIDSFryq4qpAxMIARUAAAAAGAElAADIQj0AgKJD&rs=AOn4CLA_XaIvA9VnqC2qcK58EFAmdMrS1Q',
    'https://i5.walmartimages.com/seo/Skytech-Azure-Gaming-PC-Desktop-AMD-Ryzen-7-7800X3D-NVIDIA-Geforce-RTX-5070-2TB-Gen4-NVMe-SSD-32GB-RAM-AIO-Liquid-Cooling-Windows-11_91f306ac-8b09-46f3-8570-feba113218e0.37f582a20f6174efbf56f4317992e1fe.jpeg'
  ],
  'CYB-GAM-SUP': [
    'https://m.media-amazon.com/images/I/818SNa1ruZL.jpg',
    'https://shopsimpletronics.com/cdn/shop/products/PhotoRoom_20220119_181729_1946x.png?v=1642637923',
    'https://cdn.panacompu.com/cdn-img/pv/cyberpowerpc-gamer-supreme-v6-preview.jpg?width=550&height=400&fixedwidthheight=false'
  ],
  'IBP-SLA-MSH': [
    'https://m.media-amazon.com/images/I/81xlNPKMrQL._AC_UF1000,1000_QL80_.jpg',
    'https://cdn.mos.cms.futurecdn.net/JQjSid2RWWUuieqcRfCXPC.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRhcE-zDY9Jo2ZkuWqohQxy1qJABibm1wrWpA&s'
  ],
  'INT-NUC-13': [
    'https://bizweb.dktcdn.net/thumb/1024x1024/100/329/122/products/may-tinh-mini-pc-intel-nuc-13-extreme-kit-i7-13700k-rnuc13rngi70000-6.jpg?v=1680947667597',
    'https://dlcdnwebimgs.asus.com/gain/da443026-2c2c-4101-b960-0753d97d5429/',
    'https://sb.tinhte.vn/2022/11/6199989_intel-nuc-13-extreme-raptor-canyon-tinhte-2.jpg'
  ],
  'HP-ENVY-DT': [
    'https://images-na.ssl-images-amazon.com/images/I/71fOGgAce-L.jpg',
    'https://www.notebookcheck.net/fileadmin/Notebooks/News/_nc3/HP_Envy_Desktop_Header.jpg',
    'https://support.hp.com/wcc-assets/document/images/211/c05240709.jpg'
  ],
  'DELL-INS-DT': [
    'https://cdn1615.cdn4s4.io.vn/media/products/may-tinh-de-ban/dell/inspiron/3020mt/inspiron-3020-desktop.webp',
    'https://sm.pcmag.com/pcmag_au/review/d/dell-inspi/dell-inspiron-small-desktop-3471_kz9r.jpg',
    'https://sieuviet.vn/hm_content/uploads/anh-san-pham/pc/dell/1_2.webp'
  ],
  'ACER-PRE-7K': [
    'https://cdn.assets.prezly.com/346d8126-820e-4ee0-8b60-a0077acee526/PREDATOR-ORION-7000-PO7-660-02.jpg',
    'https://file.hstatic.net/200000722513/article/predator-orion-7000-po7-640-lifestyle-03-scaled_fa4d2f201ba4484096f0f5acbcc6fc3d.jpg',
    'https://nghenhinvietnam.vn/uploads/global/quanghuy/2024/9/6/acer/nghenhin__acer-predator-orion-7000-1.jpg'
  ],
  'NZXT-PL3': [
    'https://nzxt.com/cdn/shop/files/Player-Three-Prime-ASUS-WW-10.14.25-HERO-WHITE-BADGE_cf7be002-234d-43e0-a2b8-c78f0a7b1844.png?v=1764659862',
    'https://9to5toys.com/wp-content/uploads/sites/5/2023/03/nzxt-player-three-9.png?w=1200&h=600&crop=1',
    'https://i.pcmag.com/imagery/reviews/01qbCBNLmuSqLiLmSdQY6Xk-3..v1681409799.jpg'
  ],
  'MAIN-MG1': [
    'https://cdn.mos.cms.futurecdn.net/HF5NFDnzAF8NDE8znsB5JJ.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR37aO6vYZv4QdS2NpI3Fu84JKk-NxP4Q9c3Q&s',
    'https://petapixel.com/assets/uploads/2024/06/maingear-mg-1-angle-petapixel-front.jpg'
  ],
  'ORG-NEURON': [
    'https://www.originpc.com/blog/wp-content/uploads/2019/12/neuron-hero-red.jpg',
    'https://cdn.originpc.com/img/pdp/gaming/desktops/neuron/neuron-3500x-uv-prints.jpg',
    'https://cdn.mos.cms.futurecdn.net/Et8JN39PXwXws5SzXHpCDX.jpg'
  ],
  'ASUS-ROG-DT': [
    'https://dlcdnwebimgs.asus.com/gain/95E413EB-A725-4131-82B8-FF76A880EE0D',
    'https://microless.com/cdn/products/255b85a6527a4dc4fee9a1901124670b-hi.jpg',
    'https://dlcdnwebimgs.asus.com/gain/F5A260D0-CB75-45E2-A632-521DDC5F28BE/w260/fwebp'
  ],

  // --- TVs (Updated) ---
  'LG-C3-55': [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/7/1/71557.png',
    'https://cdn11.dienmaycholon.vn/filewebdmclnew/public/userupload/files/mtsp/dien-tu/smart-tivi-lg-oled-4k-55-inch-oled55c3psa.jpg',
    'https://img.websosanh.vn/v2/users/root_product/images/oled-tivi-lg-4k-55-inch-55c3ps/219d335ca6184.jpg'
  ],
  'LG-C3-65': [
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQWKs4_XrvUIeDRFuognIRCTV-7JIUCPgwRWw&s',
    'https://www.cnet.com/a/img/resize/ded6920d086391e7e24b8e8a77a64e4031a0002f/hub/2023/06/09/b3b35284-1d0c-4c27-8676-954abc2fad8a/lg-c3-oled-tv-2023-07.jpg?auto=webp&fit=crop&height=1200&width=1200',
    'https://i5.walmartimages.com/asr/0680c9f0-304d-4b92-85ca-51cae212f046.5b1fc8050441bf9820a3c5922feffd9b.jpeg?odnHeight=768&odnWidth=768&odnBg=FFFFFF'
  ],
  'LG-G3-65': [
    'https://cdn.tgdd.vn/Products/Images/1942/306581/smart-tivi-oled-lg-4k-65-inch-65g3psa-1-700x467.jpg',
    'https://www.lg.com/content/dam/channel/wcms/vn/images/tivi/oled65g3psa_atv_eavh_vn_c/gallery/D-02.jpg',
    'https://www.cnet.com/a/img/resize/29626605caa770187edbdec3f678249ccd8c47ff/hub/2023/08/11/e2255be8-d8c3-4d3b-ada4-07454ab16b77/lg-g3-oled-tv-2023-02.jpg?auto=webp&width=1200'
  ],
  'SAM-S90C-55': [
    'https://images.samsung.com/is/image/samsung/p6pim/ph/qa55s90cagxxp/gallery/ph-oled-s90c-qa55s90cagxxp-536185455',
    'https://nghenhinvietnam.vn/uploads/global/quanghuy/2023/21/samsung/nghenhinvietnam_tv_samsung_s90c_3.jpg',
    'https://nghenhinvietnam.vn/uploads/global/quanghuy/2023/21/samsung/nghenhinvietnam_tv_samsung_s90c_1.jpg'
  ],
  'SAM-S90C-65': [
    'https://images.samsung.com/is/image/samsung/p6pim/ae/qa65s90cauxzn/gallery/ae-oled-tv-qa65s90cauxzn-front-black-titanium-536504295',
    'https://dienmay247.com.vn/wp-content/uploads/2024/01/s90c-b1-600x400-1.jpg',
    'https://i.insider.com/652e8e998bed706e837e201b?width=700'
  ],
  'SAM-QN90C-65': [
    'https://images.samsung.com/is/image/samsung/p6pim/ae/qa65s90cauxzn/gallery/ae-oled-tv-qa65s90cauxzn-front-black-titanium-536504295',
    'https://image.anhducdigital.vn/hightech/tivi/qn90c-2023/qn90c-5.png',
    'https://bizweb.dktcdn.net/thumb/1024x1024/100/439/998/products/qn95d-fbb22d2c-bb9b-476e-a476-dbd09e24ab69.png?v=1713174876603'
  ],
  'SONY-A80L-55': [
    'https://bizweb.dktcdn.net/thumb/1024x1024/100/425/687/products/1-a6f4dd7b-9abb-4656-bf44-d498e101dca2.jpg?v=1764054897643',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/g/o/google-tivi-oled-sony-xr-55a80l-4k-55-inch_8_.png',
    'https://sony.scene7.com/is/image/sonyglobalsolutions/TVFY23_A80L_65_WW_0_insitu_M?$productIntroPlatemobile$&fmt=png-alpha'
  ],
  'SONY-A95L-65': [
    'https://logico.com.vn/images/products/2023/03/23/original/a95l-2_1679544846.png',
    'https://cdn.mos.cms.futurecdn.net/Lk6dCXJhznwxULbqQj8U4P.jpg',
    'https://cdn.tgdd.vn/Products/Images/1942/308548/x95l-700x467.jpg'
  ],
  'TCL-QM8-65': [
    'https://i.rtings.com/assets/products/ygyrdRw8/tcl-qm8-qm850g-qled/design-medium.jpg?format=auto',
    'https://m.media-amazon.com/images/I/91WXzWVVGsL._AC_UF894,1000_QL80_.jpg',
    'https://i.rtings.com/assets/products/FxuLZKva/tcl-qm8-qm851g-qled/design-medium.jpg?format=auto'
  ],
  'HIS-U8K-65': [
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQv6ocmGcD3vR2p-W45Dt85ucV7DBxbalIrJw&s',
    'https://m.media-amazon.com/images/I/714LAhAd8RL._AC_UF894,1000_QL80_.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRVwQCb0tg9JZAMBa5StcbMndc4rdLm2Y_Rzg&s'
  ],
  'VIZ-PQ-65': [
    'https://www.vizio.com/content/dam/vizio/us/en/images/product/2020/tvs/p-series/p65q9-h1/gallery/2020_P-Series_P65Q9-H1_Front_OS_Newsweek-Best-Holiday-Gifts-2020.jpg/_jcr_content/renditions/cq5dam.web.640.480.png',
    'https://www.vizio.com/content/dam/vizio/us/en/images/product/2021/tv/p-series/p65q9-j01/gallery/2022_PQ9-Series_Carton.jpg/_jcr_content/renditions/cq5dam.web.640.480.png',
    'https://www.bhphotovideo.com/images/fb/vizio_pq65_f1_p_series_quantum_65_class_1439029.jpg'
  ],
  'ROKU-PLS-55': [
    'https://image.roku.com/w/rapid/images/meta-image/51c68c6e-4f37-4204-8bfb-6c9357793922.jpg',
    'https://www.cnet.com/a/img/resize/c7d369847505bec9468fc04f16d71a96658cbe6e/hub/2023/04/06/cb4769b9-48d9-4b08-93bd-ff961575aaa4/roku-tv-23-02.jpg?auto=webp&width=1200',
    'https://m.media-amazon.com/images/I/711mObDMN1L._AC_UF894,1000_QL80_.jpg'
  ],
  'AMZ-OMNI-65': [
    'https://m.media-amazon.com/images/I/61wsF9lZJmL._AC_UF1000,1000_QL80_.jpg',
    'https://m.media-amazon.com/images/I/81JIfxZb14L._AC_UF1000,1000_QL80_.jpg',
    'https://m.media-amazon.com/images/G/01/kindle/journeys/RLLnvZrYJrpztidamPzsM2FUe2FYBetwtpWHnjNYR6l9g3D/Mzc4NDUwYjct._CB608418078_.jpg'
  ],
  'LG-B3-55': [
    'https://www.lg.com/content/dam/channel/wcms/th/oled-tv/2023/b3-pdp-update/gallery/55-b3-a/TV-OLED-55-B3-A-Gallery-01.jpg/jcr:content/renditions/thum-1600x1062.jpeg',
    'https://www.lg.com/content/dam/channel/wcms/sg/images/tv/features/oled2023/TV-OLED-B3-02-Intro-Visual-Mobile.jpg',
    'https://cdn.nguyenkimmall.com/images/detailed/874/10055510-smart-tivi-oled-lg-4k-55-inch-oled55g3psa-2.jpg'
  ],
  'SAM-FRAME-55': [
    'https://cdnv2.tgdd.vn/mwg-static/dmx/Products/Images/1942/322680/tivi-qled-khung-tranh-samsung-4k-55-inch-qa55ls03d-1-638691037685437659-700x467.jpg',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/h/thi_t_k_ch_a_c_t_n_6__1.png',
    'https://cdn2.cellphones.com.vn/x/media/catalog/product/k/q/kq75lsb03afxkr_009_l-perspective1_black_2_1_2.png'
  ],
  'SONY-X90L-65': [
    'https://sony.scene7.com/is/image/sonyglobalsolutions/TVFY23_X90L_65_12_Beauty_I_M-1?$productIntroPlatemobile$&fmt=png-alpha',
    'https://cdn11.dienmaycholon.vn/filewebdmclnew/public/userupload/files/mtsp/dien-tu/chan-de-tivi-sony-xr65x90l.jpg',
    'https://sonyimages.blob.core.windows.net/productr/large/XR65X90LU_0.png'
  ],
  'TCL-6S-55': [
    'https://m.media-amazon.com/images/I/91ESqVq-i3L.jpg',
    'https://www.skyit-tt.com/wp-content/uploads/2022/11/91Zihsc0coL._AC_SL1500_.jpg',
    'https://www.cnet.com/a/img/resize/5b1159c61c0eaa5d3a38cba6a09b74f30de67c69/hub/2020/09/10/e78939f6-20b4-4f42-b9ef-03c1c618cca8/04-tcl-6-series-2020-65r635.jpg?auto=webp&width=768'
  ],
  'HIS-U7K-55': [
    'https://cdn.nguyenkimmall.com/images/detailed/898/10056785-google-tivi-mini-uled-hisense-4k-55inch-55u7k-1_o1mh-t9.jpg',
    'https://cdn.tgdd.vn/Products/Images/1942/321449/tivi-uled-4k-hisense-55u7k-700x467.jpg',
    'https://vcdn1-sohoa.vnecdn.net/2023/10/27/DSC06576-1698420432.jpg?w=460&h=0&q=100&dpr=2&fit=crop&s=sKQ-0Utr5YPRwFAyh-COhA'
  ],
  'SAM-CU7-43': [
    'https://images.samsung.com/is/image/samsung/p6pim/africa_en/ua43cu7000uxly/gallery/africa-en-crystal-uhd-cu7000-ua43cu7000uxly-536771150?$Q90_1248_936_F_PNG$',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Tivi/Samsung/43-inch/smart-tivi-samsung-uhd-43du7000-4k-43-inch-2024-1.jpg',
    'https://cdn.nguyenkimmall.com/images/thumbnails/290/235/detailed/1163/10057637-Smart_Tivi_Samsung_4K_43_inch_UA43DU7000KXXV__1_.jpg'
  ],
  'LG-UR9-50': [
    'https://m.media-amazon.com/images/I/91MAjR2HydL._AC_UF894,1000_QL80_.jpg',
    'https://m.media-amazon.com/images/I/519kS8lWBeL._AC_UF894,1000_QL80_.jpg',
    'https://cdn.mos.cms.futurecdn.net/kWbKLdF6AktQ8JkHqUBpRQ.jpg'
  ],
  'SONY-X80K-43': [
    'https://cdn.tgdd.vn/Products/Images/1942/274763/android-sony-4k-43-inch-kd-43x80k-180322-024040-550x340.png',
    'https://cdn.tgdd.vn/Products/Images/1942/274763/android-sony-4k-43-inch-kd-43x80k-240322-025758.jpg',
    'https://bizweb.dktcdn.net/thumb/grande/100/475/305/products/thumbnails-large-asset-plus-hierarchy-consumer-plus-assets-television-plus-assets-braviaa-plus-lcd-plus-hdtv-fy-plus-22-x80k-ecomm-plus-images-43-50-7-plus-frame-png-d40b5cae-a437-4294-a0b6-45b42d8d346f.png?v=1672991042467'
  ],
  'INS-F30-50': [
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcROQ3i7h3R7eF4Sa0DPzsRNbrnLnJ0CMH0mCQ&s',
    'https://images-na.ssl-images-amazon.com/images/I/91uarXXZ7LL.jpg',
    'https://images.techeblog.com/wp-content/uploads/2023/06/24131916/insignia-50-inch-class-f30-series-led-4k-smart-fire-tv-2023.jpg'
  ],
  'TOSH-C35-43': [
    'https://cdn.tgdd.vn/Products/Images/1942/297318/google-tivi-toshiba-4k-43-inch-43c350lp-10-550x340.jpg',
    'https://cdn.tgdd.vn/Products/Images/1942/341417/Slider/smart-tivi-toshiba-ai-4k-43-inch-43c350rp638978437940164200.jpg',
    'https://pisces.bbystatic.com/image2/BestBuy_US/images/products/5db66e97-e393-4016-bdd1-f92f2c655af2.png;maxHeight=1920;maxWidth=900?format=webp'
  ],
  'LG-C4-55': [
    'https://thegioithietbiso.com/data/product/rvo1714620647.jpg',
    'https://www.lg.com/content/dam/channel/wcms/uk/images/tvs-soundbars/oled-evo/oled2024/c4/features/oled-c4-16-ultra-slim-design-m.jpg',
    'https://i5.walmartimages.com/asr/7e1c6b5a-39b4-467a-bbac-fd344d24dd92.54897e0cb4b954bb57b10b349fc57d7f.jpeg?odnHeight=768&odnWidth=768&odnBg=FFFFFF'
  ],
  'SAM-S95D-65': [
    'https://images.samsung.com/is/image/samsung/p6pim/vn/qa65s95dakxxv/gallery/vn-oled-s95d-qa65s95dakxxv-thumb-540978937',
    'https://vcdn1-sohoa.vnecdn.net/2024/08/06/DSC00528-1722936628.jpg?w=460&h=0&q=100&dpr=2&fit=crop&s=oyFyac-HLq1Qk6AcQldsKA',
    'https://cdn11.dienmaycholon.vn/filewebdmclnew/DMCL21/Picture/News/News_expe_10229/10229.png?version=220712'
  ],
  'SONY-B9-65': [
    'https://sony.scene7.com/is/image/sonyglobalsolutions/TVFY24_UP_1_FrontWithStand_M?$productIntroPlatemobile$&fmt=png-alpha',
    'https://m.media-amazon.com/images/I/81i2PtSHrwL._AC_UF1000,1000_QL80_.jpg',
    'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT8oOMmm770XjCFbyNSMPnSoTnZcGFKDUqvCQ&s'
  ],
  'TCL-QM851-75': [
    'https://sm.pcmag.com/t/pcmag_au/review/t/tcl-qm8-cl/tcl-qm8-class-75-inch-tv-75qm851g_m1j9.1920.jpg',
    'https://cdn-fastly.ce-sphere.com/media/2024/07/09/12261/post.jpg?size=720x845&nocrop=1',
    'https://www.flatpanelshd.com/pictures/tcl_qm851g_tvd.jpg'
  ]
};

async function seed() {
  try {
    console.log('Starting database seeding...');

    // 0. Clean up existing data (Child tables first to avoid FK errors)
    console.log('Cleaning up previous data...');
    await prisma.commission.deleteMany();
    await prisma.salesTarget.deleteMany();
    await prisma.orderItem.deleteMany();
    await prisma.order.deleteMany();
    await prisma.productImage.deleteMany(); // Ensure images are deleted
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
        // ==============================================================================
        // LOGIC: Check 'productImages' map first (returns string[]).
        // If not found, use placeholder array.
        // ==============================================================================
        let imageUrls = productImages[item.sku];

        if (!imageUrls || imageUrls.length === 0) {
           imageUrls = [
            `https://placehold.co/600x400/333333/ffffff/png?text=${encodeURIComponent(item.name)}\\n(${item.sku})`
           ];
        }

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
            images: {
              create: imageUrls.map((url, index) => ({
                imageUrl: url,
                displayOrder: index,
                isMain: index === 0
              }))
            }
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
      // New customers
      { name: 'Christopher Lee', email: 'chris.lee@example.com', phone: '0913456789', address: '159 Valley Rd, Austin, TX', isMember: true, memberSince: new Date('2024-04-12') },
      { name: 'Patricia White', email: 'pat.white@example.com', phone: '0914567890', address: '753 Hill St, Nashville, TN', isMember: false },
      { name: 'Daniel Harris', email: 'dan.harris@example.com', phone: '0915678901', address: '951 Lake Dr, Detroit, MI', isMember: true, memberSince: new Date('2024-08-05') },
      { name: 'Nancy Clark', email: 'nancy.clark@example.com', phone: '0916789012', address: '357 River Ln, Minneapolis, MN', isMember: false },
      { name: 'Matthew Lewis', email: 'matt.lewis@example.com', phone: '0917890123', address: '486 Forest Ave, Tampa, FL', isMember: true, memberSince: new Date('2024-09-18') },
      { name: 'Karen Walker', email: 'karen.w@example.com', phone: '0918901234', address: '624 Mountain Rd, Cleveland, OH', isMember: false },
      { name: 'Thomas Hall', email: 'tom.hall@example.com', phone: '0919012345', address: '792 Beach Blvd, San Diego, CA', isMember: true, memberSince: new Date('2024-01-25') },
      { name: 'Betty Allen', email: 'betty.allen@example.com', phone: '0920123456', address: '135 Park Ave, Philadelphia, PA', isMember: false },
      { name: 'Charles Young', email: 'charles.y@example.com', phone: '0921234567', address: '246 Garden St, Charlotte, NC', isMember: true, memberSince: new Date('2024-06-30') },
      { name: 'Sandra King', email: 'sandra.king@example.com', phone: '0922345678', address: '864 Sunset Dr, Indianapolis, IN', isMember: true, memberSince: new Date('2024-03-08') },
    ];
    const customers: Customer[] = [];
    for (const cus of customerData) {
      customers.push(await prisma.customer.create({ data: cus }));
    }

    // 6. Orders
    console.log('Seeding orders...');
    let completedOrdersCount = 0;
    if (allCreatedProducts.length > 0 && customers.length > 0) {
      const getRandomProduct = () => allCreatedProducts[Math.floor(Math.random() * allCreatedProducts.length)];
      const getRandomCustomer = () => customers[Math.floor(Math.random() * customers.length)];
      const getRandomUser = () => users[Math.floor(Math.random() * users.length)];
      const getRandomDate = (start: Date, end: Date) => new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));

      const statuses = ['COMPLETED', 'COMPLETED', 'COMPLETED', 'PROCESSING', 'PENDING', 'CANCELLED'];
      const numberOfOrdersToSeed = 100;
      const COMMISSION_RATE = 3; // 3% commission

      for (let i = 1; i <= numberOfOrdersToSeed; i++) {
        const orderDate = getRandomDate(new Date('2024-01-01'), new Date());
        const orderNum = `ORD-2024-${i.toString().padStart(3, '0')}`;
        const status = statuses[Math.floor(Math.random() * statuses.length)];
        const selectedUser = getRandomUser();
        const numItems = Math.floor(Math.random() * 4) + 1;
        const items = [];
        for (let j = 0; j < numItems; j++) {
          const prod = getRandomProduct();
          items.push({ productId: prod.id, quantity: Math.floor(Math.random() * 2) + 1, unitPrice: prod.price });
        }
        const subtotal = items.reduce((sum, item) => sum + Number(item.unitPrice) * item.quantity, 0);

        const createdOrder = await prisma.order.create({
          data: {
            orderNumber: orderNum, customerId: getRandomCustomer().id, userId: selectedUser.id, status: status as any,
            subtotal: new Prisma.Decimal(subtotal), discountAmount: new Prisma.Decimal(0), taxAmount: new Prisma.Decimal(0),
            total: new Prisma.Decimal(subtotal), createdAt: orderDate,
            completedAt: status === 'COMPLETED' ? orderDate : null,
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

        // Create Commission for COMPLETED orders
        if (status === 'COMPLETED') {
          const commissionAmount = subtotal * (COMMISSION_RATE / 100);
          await prisma.commission.create({
            data: {
              userId: selectedUser.id,
              orderId: createdOrder.id,
              orderTotal: new Prisma.Decimal(subtotal),
              commissionRate: new Prisma.Decimal(COMMISSION_RATE),
              commissionAmount: new Prisma.Decimal(commissionAmount),
              isPaid: Math.random() > 0.5, // 50% chance of being paid
              paidAt: Math.random() > 0.5 ? orderDate : null,
              createdAt: orderDate
            }
          });
          completedOrdersCount++;
        }
      }
      console.log(`Created ${numberOfOrdersToSeed} orders`);
      console.log(`Created ${completedOrdersCount} commissions for completed orders`);

      // Update customer totalSpent based on completed orders
      console.log('Updating customer totalSpent...');
      for (const customer of customers) {
        const completedOrders = await prisma.order.findMany({
          where: {
            customerId: customer.id,
            status: 'COMPLETED',
          },
        });

        const totalSpent = completedOrders.reduce((sum, order) => {
          return sum + Number(order.total);
        }, 0);

        await prisma.customer.update({
          where: { id: customer.id },
          data: { totalSpent: new Prisma.Decimal(totalSpent) },
        });
      }
      console.log('Customer totalSpent updated successfully');
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