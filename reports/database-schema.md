# Tables

## User

| Column Name | Data Type | Constraints | Description   |
| ----------- | --------- | ----------- | ------------- |
| Id          | TEXT      | PRIMARY KEY | Firebase UID  |
| Email       | TEXT      | NOT NULL    | User's email  |
| DisplayName | TEXT      | NULL        | Display name  |
| CreatedAt   | TEXT      | NOT NULL    | Creation date |

## Category

| Column Name   | Data Type | Constraints                      | Description                      |
| ------------- | --------- | -------------------------------- | -------------------------------- |
| Id            | INTEGER   | PRIMARY KEY AUTOINCREMENT        | Unique category ID               |
| UserId        | TEXT      | NOT NULL, FOREIGN KEY → User(Id) | Owner of this category           |
| Name          | TEXT      | NOT NULL                         | Category name                    |
| Icon          | TEXT      | NULL                             | Icon identifier                  |
| Color         | TEXT      | NULL                             | Hex color code                   |
| Type          | TEXT      | NOT NULL                         | "Income" or "Expense"            |
| MonthlyBudget | DECIMAL   | NULL                             | Monthly budget limit             |
| IsActive      | INTEGER   | NOT NULL, DEFAULT 1              | 1 = active, 0 = deleted/archived |
| CreatedAt     | TEXT      | NOT NULL                         | Creation date                    |

**Notes:**

- Categories are user-specific (each user has their own categories)
- Default categories are created automatically during user registration

## Transaction

| Column Name | Data Type | Constraints                          | Description                                  |
| ----------- | --------- | ------------------------------------ | -------------------------------------------- |
| Id          | INTEGER   | PRIMARY KEY AUTOINCREMENT            | Unique transaction ID                        |
| UserId      | TEXT      | NOT NULL, FOREIGN KEY → User(Id)     | Owner of this transaction                    |
| CategoryId  | INTEGER   | NOT NULL, FOREIGN KEY → Category(Id) | Associated category                          |
| Amount      | decimal   | NOT NULL                             | Transaction amount (always positive)         |
| Type        | TEXT      | NOT NULL                             | "Income" or "Expense"                        |
| Date        | TEXT      | NOT NULL                             | Transaction date                             |
| Description | TEXT      | NULL                                 | Optional note/description                    |
| PhotoPath   | TEXT      | NULL                                 | Local file path to receipt photo             |
| IsRecurring | INTEGER   | NOT NULL, DEFAULT 0                  | 1 if from recurring transaction, 0 if manual |
| CreatedAt   | TEXT      | NOT NULL                             | Record creation timestamp                    |
| UpdatedAt   | TEXT      | NULL                                 | Last update timestamp                        |

**Notes:**

- PhotoPath stores local path to receipt photo taken with camera
- IsRecurring indicates if transaction was auto-generated from RecurringTransaction

## RecurringTransaction

| Column Name       | Data Type | Constraints                          | Description                               |
| ----------------- | --------- | ------------------------------------ | ----------------------------------------- |
| Id                | INTEGER   | PRIMARY KEY AUTOINCREMENT            | Unique recurring transaction ID           |
| UserId            | TEXT      | NOT NULL, FOREIGN KEY → User(Id)     | Owner of this transaction                 |
| CategoryId        | INTEGER   | NOT NULL, FOREIGN KEY → Category(Id) | Associated category                       |
| Amount            | decimal   | NOT NULL                             | Fixed amount                              |
| Type              | TEXT      | NOT NULL                             | "Income" or "Expense"                     |
| Description       | TEXT      | NOT NULL                             | Description                               |
| Frequency         | TEXT      | NOT NULL                             | "Daily", "Weekly", "Monthly", "Yearly"    |
| DayOfMonth        | INTEGER   | NULL                                 | Day of month (1-31) for monthly recurring |
| DayOfWeek         | INTEGER   | NULL                                 | Day of week (1-7) for weekly              |
| IsActive          | INTEGER   | NOT NULL, DEFAULT 1                  | 1 = active, 0 = paused/deleted            |
| LastProcessedDate | TEXT      | NULL                                 | Last date this was auto-added             |
| NextDueDate       | TEXT      | NULL                                 | Next scheduled date                       |
| CreatedAt         | TEXT      | NOT NULL                             | Creation date                             |

**Notes:**

- Recurring transactions automatically create Transaction records on their due date
- Use IsActive to pause subscriptions without deleting them

## Relations

User (1) ──< (N) Transaction
User (1) ──< (N) Category
User (1) ──< (N) RecurringTransaction

Category (1) ──< (N) Transaction
Category (1) ──< (N) RecurringTransaction
