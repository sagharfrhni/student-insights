import sqlite3
import os

def init_mock_db():
    current_dir = os.path.dirname(os.path.abspath(__file__))
    db_path = os.path.join(current_dir, 'local_test.db')
    
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()


    cursor.execute('''
    CREATE TABLE IF NOT EXISTS Courses (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER,
        Name TEXT
    )''')

   
    cursor.execute('''
    CREATE TABLE IF NOT EXISTS StudyLogs (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER,
        CourseId INTEGER,
        StudyDateUtc TEXT,
        DurationMinutes INTEGER,
        Notes TEXT
    )''')

    cursor.execute('''
    CREATE TABLE IF NOT EXISTS LearningActivities (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER,
        CourseId INTEGER,
        Title TEXT,
        Status TEXT,
        DueDateUtc TEXT
    )''')


    cursor.execute('''
    CREATE TABLE IF NOT EXISTS Exams (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER,
        CourseId INTEGER,
        Title TEXT,
        Grade REAL,
        ExamDateUtc TEXT
    )''')

    cursor.execute("DELETE FROM Courses")
    cursor.execute("DELETE FROM StudyLogs")
    cursor.execute("DELETE FROM LearningActivities")
    cursor.execute("DELETE FROM Exams")


    cursor.executemany("INSERT INTO Courses (Id, UserId, Name) VALUES (?, ?, ?)", [
        (101, 1, 'ریاضی عمومی'),
        (102, 1, 'پایگاه داده'),
        (103, 1, 'هوش مصنوعی')
    ])


    cursor.executemany("INSERT INTO StudyLogs (UserId, CourseId, StudyDateUtc, DurationMinutes) VALUES (?, ?, ?, ?)", [
        (1, 101, '2026-07-20', 120),
        (1, 101, '2026-07-21', 90),
        (1, 102, '2026-07-22', 60),
        (1, 103, '2026-07-23', 180)
    ])


    cursor.executemany("INSERT INTO LearningActivities (UserId, CourseId, Title, Status) VALUES (?, ?, ?, ?)", [
        (1, 101, 'تمرین ۱ ریاضی', 'Completed'),
        (1, 101, 'تمرین ۲ ریاضی', 'Pending'),
        (1, 102, 'پروژه دیتابیس', 'Completed')
    ])
 
    cursor.executemany("INSERT INTO Exams (UserId, CourseId, Title, Grade, ExamDateUtc) VALUES (?, ?, ?, ?, ?)", [
        (1, 101, 'میان‌ترم ریاضی', 18.5, '2026-07-10'),
        (1, 102, 'میان‌ترم دیتابیس', 14.0, '2026-07-15')
    ])

    conn.commit()
    conn.close()
    print("The local database was created, and the data was successfully added.")

if __name__ == '__main__':
    init_mock_db()