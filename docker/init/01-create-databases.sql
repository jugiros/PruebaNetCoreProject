CREATE DATABASE IF NOT EXISTS fintech_auth_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
GRANT ALL PRIVILEGES ON fintech_auth_db.* TO 'fintech'@'%';
FLUSH PRIVILEGES;
