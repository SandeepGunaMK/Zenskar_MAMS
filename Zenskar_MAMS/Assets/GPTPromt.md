# 🥋 Zenskar Martial Arts Management System — WPF Application Specification

## 🎯 Objective
Build a **WPF desktop application** named `Zenskar_MAMS` for managing a Martial Arts academy.  
The application should be role-based with login functionality for **Admin**, **Masters**, and **Instructors**, and provide access to manage **students**, **requests**, and **user registrations**.

---

## 🗄️ DATABASE DETAILS

### Database Configuration
- **Server:** localhost  
- **Authentication:** Windows Authentication  
- **Database Name:** `Zenskar_DB`

### Database Requirements
Copilot, generate **SQL scripts** for the required tables and columns based on the following details.  
Wait for confirmation before proceeding with table creation.

#### Required Tables (Minimum)
1. **User_Table**
   - User_ID (INT, Primary Key, Identity)
   - login_ID (NVARCHAR(7))
   - User_Name (NVARCHAR(100))
   - Contact_Number (NVARCHAR(15))
   - Password (NVARCHAR(100))
   - User_Type (NVARCHAR(50)) — [Admin, Master, Instructor]
   - Status (NVARCHAR(50)) — [Inactive, Active, Pending Approval]
   - Created_Date (DATETIME)
   - Approved_By (NVARCHAR(100), Nullable)
   - Approved_Date (DATETIME, Nullable)

2. **Student_Data**
   - Student_ID (INT, Primary Key, Identity)
   - Name (NVARCHAR(100))
   - DOB (DATE)
   - Age (INT)
   - Gender (NVARCHAR(10))
   - Location (NVARCHAR(100))
   - Belt (NVARCHAR(50))
   - InstructorName (NVARCHAR(100))
   - MasterName (NVARCHAR(100))
   - ContactNumber (NVARCHAR(15))
   - ParentsName (NVARCHAR(100))
   - MedicalConditions (NVARCHAR(255))
   - LastExamDate (DATE)
   - Attempts (INT)
   - DateOfJoining (DATE)
   - Comments (NVARCHAR(255))
   - StudentStatus (NVARCHAR(50)) — [Stopped, Active] - Active while inserting a new Student

3. **Requests**
   - Request_ID (INT, Primary Key, Identity)
   - RequestType (NVARCHAR(50)) — [Update, Delete, Registration]
   - RequestedBy (NVARCHAR(100))
   - Student_ID (INT, Nullable, Foreign Key)
   - Status (NVARCHAR(50)) — [Open, Approved, Rejected]
   - RejectedReason (NVARCHAR(255), Nullable)
   - RequestedDate (DATETIME)
   - ApprovedBy (NVARCHAR(100), Nullable)
   - ApprovedDate (DATETIME, Nullable)

---

## ⚙️ DATABASE CONNECTIVITY

- Place the **connection string** in `App.config`.
- Use **SqlConnection** for all DB operations.
- Create a **DBContext.cs** file with the following 5 methods:

| Method | Description |
|---------|--------------|
| `CreateConnection()` | Establish and return a SqlConnection object. |
| `SelectData(string query)` | Execute SELECT queries and return results as DataTable. |
| `InsertData(string query, SqlParameter[])` | Execute INSERT statements with parameters. |
| `UpdateData(string query, SqlParameter[])` | Execute UPDATE statements with parameters. |
| `DeleteData(string query, SqlParameter[])` | Execute DELETE statements with parameters. |

All methods should use **parameterized queries** to prevent SQL injection.

---

## 🎨 APPLICATION THEME

- The theme should be inspired by the **uploaded logo** (`/Assets/logo.png`).
- Use **Navy Blue (#001F3F)** and **White (#FFFFFF)** as the primary colors.
- Create a folder named `/Theme` containing:
  - `Colors.xaml` → Define color palette.
  - `Styles.xaml` → Define control styles (buttons, textboxes, etc.).
  - `AppTheme.xaml` → Merge themes and apply globally.

Design must be **sleek, modern, and interactive**.  
Add soft animations for transitions and splash screen fade-ins.

---

## 🪟 APPLICATION WINDOWS / PAGES

### 1. SplashScreen.xaml
- Display the logo with a **transparency or fade effect**.
- Automatically navigate to the Login window after a short delay of 3 seconds.

### 2. Login.xaml
- Fields: **login_ID** for the login_ID, **Password**
- Button: **Login**, **Register New User**
- Authenticate using `User_Table`.
- Redirect users based on **User_Type**:
  - Admin → `AdminHome.xaml`
  - Master → `MasterHome.xaml`
  - Instructor → `InstructorHome.xaml`

### 3. Register.xaml
- Fields:
  - User Name
  - Contact Number
  - New Password
  - Confirm Password
  - User Type: (Radio buttons for Master / Instructor)
- On submit:
  - The login_ID should be auto-generated during registration, derived from the user's name, and ensured to be unique within the table and should be displayed to the user.
  - Create a **registration request** entry for admin approval in the `Requests` table.
  - Only approved users can log in.

---

## 🏠 HOME PAGES

### AdminHome.xaml
- Buttons: Create a transparency interactive effected buttons filling the page
  - `Students List`
  - `Requests`
  - `Manage Users`
- Permissions:
  - Full CRUD on students.
  - Approve all types of requests.

### MasterHome.xaml
- Buttons: Create a transparency interactive effected buttons filling the page
  - `Students List`
  - `Requests`
- Permissions:
  - Select, Insert, Update students.
  - Can request **Deletion** of a student (Admin approves).
  - Approve Updation requests from a Instructor.

### InstructorHome.xaml
- Buttons: Create a transparency interactive effected buttons filling the page 
  - `Students List`
  - `Requests`
- Permissions:
  - Select, Insert students.
  - Can **Update/Delete Requests**.
  - Can Directly update details of students whose `InstructorName` matches the current user. for other students the user must raise a Updation request

---

## 📋 REQUESTS PAGE

- Include filters (checkboxes) for:
  - **Open** → Pending requests
  - **Approved**
  - **Rejected** → Show reason
- Approve/Reject buttons visible only to authorized users.

---

## 🧑‍🎓 STUDENTS LIST PAGE

### Display Columns:
- Student Name, DOB, Age, Gender, Location, Belt, InstructorName, MasterName, StudentStatus
### Features:
- **Search filter** 
	- (by Student Name,Age, Gender, Location, Belt, InstructorName, MasterName, StudentStatus) 
	- For filter in age above or below or equal to specified age should be displayed.
	- Add a + button so add multiple Filters [User can choose (or)/(and) in between filters]
- **On row click** → Open `StudentDetails.xaml`
### Actions: 
| Action | Admin | Master | Instructor |
|---------|--------|---------|-------------|
| Insert | ✅ | ✅ | ✅ |
| Update | ✅ | ✅ | Only if InstructorName = current user |
| Delete | ✅ | ❌ | ❌ |
| Stop | ✅ | ✅ | ✅ | - should update the StudentStatus of the particular student as [Stopped]
| Update Request | ❌ | ❌ | ✅ |
| Delete Request | ❌ | ✅ | ✅ |

---
## 🧾 STUDENT DETAILS PAGE

Display:
- Name, DOB, Age, Gender, Location, Belt, InstructorName, MasterName, Contact Number, Parents Name, Medical Conditions, Last Exam Date, Attempts, Date of Joining, Comments, Student Status.

Include buttons for: (depending on role)
- Edit
- Save
- Delete
- Request Update {Get the details that need to be updated and create a request}
- Request Delete

---
## 👤 UserDetailsWindow
Purpose / Function:
Display a list of users (Masters and Instructors) with their details. Allow the Admin to manage users, including deleting them.

Access:
Only Admin can open this window.

Buttons / Actions:
Delete User → deletes the selected user (with confirmation prompt).
Close / Back → closes this window and returns to Admin Home.

Navigation:
A button on Admin Home Page (Manage Users) will open UserDetailsWindow.
Recommended: Use ShowDialog() to open it so Admin must close it before returning.

---

## 📁 FOLDER STRUCTURE

/ZenskarApp
│
├── DBContext.cs
├── App.config
│
├── /Theme
│ ├── Colors.xaml
│ ├── Styles.xaml
│ └── AppTheme.xaml
│
├── /Windows
│ ├── SplashScreen.xaml
│ ├── Login.xaml
│ ├── Register.xaml
│ ├── AdminHome.xaml
│ ├── MasterHome.xaml
│ ├── InstructorHome.xaml
│ ├── StudentList.xaml
│ ├── StudentDetails.xaml
│ └── Requests.xaml
│
├── /Assets
│ └── logo.png
│
└── README.md


---

## 💡 ADDITIONAL NOTES

- Use **MVVM** pattern where applicable.
- Use **ObservableCollection** for binding to UI lists.
- Add **message boxes** for all CRUD success/failure messages.
- Add **animations** for splash screen and page transitions.
- The login_ID should be auto-generated during registration, derived from the user's name, and ensured to be unique within the table.

---

## 🔥 PROMPT FOR GITHUB COPILOT

> Create a complete WPF application named `Zenskar_MAMS` using C# and XAML that follows the specifications in this file.  
> Implement SQL Server database connectivity (local Windows Authentication).  
> Build all pages, classes, and theme files as per the folder structure.  
> Generate SQL scripts for the required tables in the `Zenskar_DB` database and wait for confirmation before proceeding to App implementation.  
> Use parameterized queries, MVVM structure where possible, and apply the navy blue & white theme with the provided logo as the central branding.