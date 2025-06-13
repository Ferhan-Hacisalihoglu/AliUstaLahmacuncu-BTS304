**GitHub Description for Ali Usta Lahmacuncu Management System**

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

### Screenshots

![Picture1](https://github.com/user-attachments/assets/67699e6c-30c3-4f9a-b96e-b2ebad328775)
![Picture2](https://github.com/user-attachments/assets/200db5aa-d5db-4e0f-9dcd-f08def4c6574)
![Picture3](https://github.com/user-attachments/assets/73dd3104-9f0e-402c-8da3-305cac6977b8)
![Picture4](https://github.com/user-attachments/assets/012393ef-4968-46c3-bbca-ce3778463db5)
![Picture5](https://github.com/user-attachments/assets/b67ae22b-a3ea-43a8-b034-9fdb68b04c04)
![Picture6](https://github.com/user-attachments/assets/4393d479-d4d2-42aa-9a3a-9ca42a72800c)
![Picture7](https://github.com/user-attachments/assets/197d3584-1123-4d41-bac9-3ffd8bdb6bb8)
![Picture8](https://github.com/user-attachments/assets/a1a000e7-6366-43bd-9147-64091251b120)
![Picture9](https://github.com/user-attachments/assets/7af45bb8-47b0-4f70-9e65-425d553137b0)
![Picture10](https://github.com/user-attachments/assets/007fde1f-6e0f-4012-89ea-dbe7fe27e934)
![Picture11](https://github.com/user-attachments/assets/c76c963e-78cd-48c0-9cb2-ef99f5af8d27)
![Picture12](https://github.com/user-attachments/assets/c3656473-8c2e-4d45-a971-52a4adbaa4af)


This project was created as a school assignment for the **Database Management Systems-2** course at Bartın University, Faculty of Science, Department of Computer Technology and Information Systems.

