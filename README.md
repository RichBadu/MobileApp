Trackeroo 

A cross-platform budget tracking app built with .NET MAUI. Track your income and expenses, set budgets per category, automate recurring transactions and see where your money goes with clear statistics, online or offline.

Built as my final project for the Mobile Development program at Howest University of Applied Sciences (Belgium).

Features
Transactions: add, edit and delete income and expenses, with optional receipt photos taken with the camera
Categories: organise spending and set a budget per category
Budget warnings: get notified when you're close to or over a category budget
Recurring transactions: automate rent, subscriptions and salary
Statistics: charts showing spending per category and across multiple months
Offline-first: works without internet and syncs automatically once you're back online
Authentication: secure login and registration with automatic token refresh
Light and dark mode: full theme support through a semantic design system

Architecture

Trackeroo follows an offline-first approach. All data is written to a local SQLite database first, so the app stays fast and usable without a connection. A sync service pushes local changes to Supabase and pulls remote updates when the device is online.
