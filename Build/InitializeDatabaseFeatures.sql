-- MySQL5 Only, Features
-- Tests Stored Procedures

DROP DATABASE IF EXISTS simpledatatest;
CREATE DATABASE simpledatatest;
SHOW DATABASES;

USE simpledatatest;

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
USE `simpledatatest`$$
CREATE PROCEDURE `GetAllCustomers` ()
BEGIN
	SELECT * from customers order by CustomerId;
END$$

DELIMITER ;

DROP procedure IF EXISTS `GetCustomerById`;

DELIMITER $$
USE `simpledatatest`$$
CREATE PROCEDURE `GetCustomerById` (id int)
BEGIN
	select * from customers where CustomerId = id;
END$$

DELIMITER ;

DROP procedure IF EXISTS `GetCustomersByName`;

DELIMITER $$

CREATE DEFINER=`root`@`%` PROCEDURE `GetCustomersByName`(_name varchar(255))
BEGIN
	select * from customers where Name = _name;
END$$

DELIMITER ;

DROP procedure IF EXISTS `GetCountCustomersAsOutputParam`;

DELIMITER $$
USE `simpledatatest`$$
CREATE PROCEDURE `GetCountCustomersAsOutputParam` (out answer int)
BEGIN
	SELECT count(CustomerId)
    INTO answer
    FROM customers;
END$$

DELIMITER ;
