**GitHub Description for Ali Usta Lahmacuncu Management System (B1 English Level)**

This is a database management system for "Ali Usta Lahmacuncu," a shop that takes custom orders, produces food with different ingredients, and manages everything from ingredient supply to delivery. The project was developed using **MySQL** for the database and **ASP.NET MVC** for the web application.

### Features
- **Customer Management**: Stores customer details like name, surname, phone, and email.
- **Order Management**: Tracks orders with details like product, quantity, price, chef, courier, and date.
- **Product Management**: Manages products with their names, prices, descriptions, and categories.
- **Ingredient Management**: Keeps track of ingredients, stock quantities, and suppliers.
- **Supplier Management**: Stores supplier information and tracks ingredient purchases.
- **Staff Management**: Manages staff details, including tasks, salaries, and contact information.
- **Comment System**: Allows customers to leave comments and ratings for orders.
- **Address Management**: Stores multiple addresses for each customer.

### Database Structure
The system uses a relational database with tables like:
- **Product**: ID, Name, Price, Description, Category
- **Ingredient**: ID, Name, Stock Quantity
- **Supplier**: ID, Name, Phone, Email, Ingredient ID
- **Order**: ID, Customer ID, Product ID, Chef ID, Courier ID, Quantity, Price, Date
- **Comment**: ID, Customer ID, Order ID, Rating, Description
- **Staff**: ID, Name, Surname, Phone, Email, Salary, Task ID
- **Purchase**: ID, Supplier ID, Date, Ingredient ID, Quantity
- **Address**: ID, Description, Customer ID
- **Category**: ID, Name
- **Admin**: ID, Name, Password

### Relationships
- One customer can have many orders (1-N).
- One product can have many ingredients, and one ingredient can be used in many products (N-N).
- Each order has one chef and one courier (1-1).
- One customer can have multiple addresses and comments (1-N).
- Each ingredient is supplied by one supplier, but suppliers can provide multiple ingredients (1-N).

### Purpose
This system helps Ali Usta manage orders, ingredients, staff, and customers in a digital and organized way. It makes the shop's operations easier and more efficient.

### Technologies
- **Backend**: ASP.NET MVC
- **Database**: MySQL
- **Other**: Entity-Relationship (ER) diagram and logical schema design

This project was created as a school assignment for the **Database Management Systems-2** course at Bartın University, Faculty of Science, Department of Computer Technology and Information Systems.

---

Feel free to check the code and contribute! For any questions, contact me.
