use ucubot;
CREATE TABLE IF NOT EXISTS student(
    id INT NOT NULL auto_increment PRIMARY KEY,
    first_name VARCHAR(128),
    last_name VARCHAR(128),
	  user_id VARCHAR(128) UNIQUE);