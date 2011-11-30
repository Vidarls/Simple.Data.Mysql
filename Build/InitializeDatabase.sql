DROP DATABASE IF EXISTS SimpleDataTest;
CREATE DATABASE SimpleDataTest;
SHOW DATABASES;
USE SimpleDataTest;
-- MySQL dump 9.11
--
-- Host: localhost    Database: simpledatatest
-- ------------------------------------------------------
-- Server version	4.0.26-nt

--
-- Table structure for table `customers`
--

CREATE TABLE customers (
  CustomerId int(11) NOT NULL auto_increment,
  Name varchar(255) default NULL,
  Address varchar(255) default NULL,
  PRIMARY KEY  (CustomerId)
) ENGINE=MyISAM;

--
-- Dumping data for table `customers`
--

INSERT INTO customers VALUES (1,'Alice','1, Street');

--
-- Table structure for table `items`
--

CREATE TABLE items (
  ItemId int(11) NOT NULL auto_increment,
  Name varchar(255) default NULL,
  Price decimal(10,2) default NULL,
  PRIMARY KEY  (ItemId)
) ENGINE=MyISAM;

--
-- Dumping data for table `items`
--

INSERT INTO items VALUES (1,'Widget','9.99');
INSERT INTO items VALUES (2,'Flange','1.00');

--
-- Table structure for table `orderitems`
--

CREATE TABLE orderitems (
  OrderItemId int(11) NOT NULL auto_increment,
  OrderId int(11) default NULL,
  ItemId int(11) default NULL,
  Quantity int(11) default NULL,
  PRIMARY KEY  (OrderItemId)
) ENGINE=MyISAM;

--
-- Dumping data for table `orderitems`
--

INSERT INTO orderitems VALUES (1,1,1,1);
INSERT INTO orderitems VALUES (2,1,2,2);

--
-- Table structure for table `orders`
--

CREATE TABLE orders (
  OrderId int(11) NOT NULL auto_increment,
  OrderDate datetime default NULL,
  CustomerId int(11) default NULL,
  PRIMARY KEY  (OrderId)
) ENGINE=MyISAM;

--
-- Dumping data for table `orders`
--

INSERT INTO orders VALUES (1,'2010-08-11 00:00:00',1);

--
-- Table structure for table `users`
--

CREATE TABLE users (
  Id int(11) NOT NULL auto_increment,
  Name varchar(255) default NULL,
  Password varchar(255) default NULL,
  Age int(11) default NULL,
  PRIMARY KEY  (Id)
) ENGINE=MyISAM;

--
-- Dumping data for table `users`
--

INSERT INTO users VALUES (1,'Bob','Secret',42);
INSERT INTO users VALUES (2,'Steve','Squirrel',69);
INSERT INTO users VALUES (3,'Dave','Password',12);
INSERT INTO users VALUES (22,'Alice','foo',29);


CREATE TABLE orders_fk_test (
  OrderId int(11) NOT NULL auto_increment,
  OrderDate datetime default NULL,
  CustomerId int(11) default NULL,
  PRIMARY KEY  (OrderId)
) ENGINE=InnoDB;

CREATE TABLE items_fk_test (
  ItemId int(11) NOT NULL auto_increment,
  Name varchar(255) default NULL,
  Price decimal(10,2) default NULL,
  PRIMARY KEY  (ItemId)
) ENGINE=InnoDB;

CREATE TABLE orderitems_fk_test (
  OrderItemId int(11) NOT NULL auto_increment,
  OrdersId int(11) default NULL,
  ItemsId int(11) default NULL,
  Quantity int(11) default NULL,
  PRIMARY KEY  (OrderItemId),
  INDEX idx_test_orderitems_items(ItemsId),
  INDEX idx_test_orderitems_orders(OrdersId),
  CONSTRAINT fk_test_orderitems_items FOREIGN KEY (ItemsId) REFERENCES items_fk_test(ItemId),
  CONSTRAINT fk_test_orderitems_orders FOREIGN KEY (OrdersId) REFERENCES orders_fk_test(OrderId)
) ENGINE=InnoDB;

SHOW TABLES;