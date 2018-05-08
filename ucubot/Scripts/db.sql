CREATE DATABASE ucubot;
CREATE user 'admin'@'%' IDENTIFIED BY 'password';
GRANT ALL PRIVILEGES ON ucubot.* TO 'admin'@'%';
FLUSH PRIVILEGES;