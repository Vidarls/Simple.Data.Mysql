-- MySQL5 Only, Features
-- Tests Stored Procedures

DROP DATABASE IF EXISTS simpledatatestfeatures;
CREATE DATABASE simpledatatestfeatures;
SHOW DATABASES;

USE simpledatatestfeatures;

CREATE TABLE customers (
  CustomerId int(11) NOT NULL auto_increment,
  Name varchar(255) default NULL,
  Address varchar(255) default NULL,
  PRIMARY KEY  (CustomerId)
) ENGINE=MyISAM;

INSERT INTO customers VALUES (1,'Alice','1, Street');
INSERT INTO customers VALUES (2,'Albert','2, Street');
INSERT INTO customers VALUES (3,'Amy','3, Street');
INSERT INTO customers VALUES (4,'Andrew','4, Street');
INSERT INTO customers VALUES (5,'Amy','5, Street');

DROP procedure IF EXISTS `GetAllCustomers`;

DELIMITER $$
CREATE PROCEDURE `GetAllCustomers` ()
BEGIN
	SELECT * from customers order by CustomerId;
END$$

DELIMITER ;

DROP procedure IF EXISTS `GetCustomerById`;

DELIMITER $$
CREATE PROCEDURE `GetCustomerById` (id int)
BEGIN
	select * from customers where CustomerId = id;
END$$

DELIMITER ;

DROP procedure IF EXISTS `GetCustomersByName`;

DELIMITER $$

CREATE PROCEDURE `GetCustomersByName`(_name varchar(255))
BEGIN
	select * from customers where Name = _name;
END$$

DELIMITER ;

DROP procedure IF EXISTS `GetCountCustomersAsOutputParam`;

DELIMITER $$
CREATE PROCEDURE `GetCountCustomersAsOutputParam` (out answer int)
BEGIN
	SELECT count(CustomerId)
    INTO answer
    FROM customers;
END$$

DELIMITER ;

CREATE TABLE orders (
  OrderId int(11) NOT NULL auto_increment,
  Created datetime default NULL,
  BigNum bigint default NULL,
  `Status` tinyint default NULL,
  PRIMARY KEY  (OrderId)
) ENGINE=MyISAM;

INSERT INTO orders VALUES (1,'2014-01-10 10:00:00', 9223372036854775807, 1);
INSERT INTO orders VALUES (2,'2014-01-11 10:00:00', 9223372036854775806, 2);
INSERT INTO orders VALUES (3,'2014-01-12 10:00:00', 9223372036854775805, 3);
INSERT INTO orders VALUES (4,'2014-01-13 10:00:00', 9223372036854775807, 4);
INSERT INTO orders VALUES (5,'2014-01-14 10:00:00', 9223372036854775804, 3);
INSERT INTO orders VALUES (6,'2014-01-15 10:00:00', 9223372036854775803, 2);
INSERT INTO orders VALUES (7,'2014-01-16 10:00:00', 9223372036854775802, 1);
INSERT INTO orders VALUES (8,'2014-01-10 10:01:00', 9223372036854775801, 2);
INSERT INTO orders VALUES (9,'2014-01-10 10:02:00', 9223372036854775804, 4);

DROP procedure IF EXISTS `GetOrdersFromADate`;

DELIMITER $$
CREATE PROCEDURE `GetOrdersFromADate` (dateFrom dateTime)
BEGIN
	SELECT *
    FROM orders
	WHERE Created > dateFrom;
END$$

DELIMITER ;
