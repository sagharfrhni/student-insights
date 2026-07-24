import sqlite3

def init_mock_db():
    conn = sqlite3.connect('local_test.db')
    cursor = conn.cursor()

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
        Status TEXT, -- 'Pending', 'Completed'
        DueDateUtc TEXT
    )''')


    cursor.execute('''
    CREATE TABLE IF NOT EXISTS Exams (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER,
        CourseId INTEGER,
        Title TEXT,
        Grade REAL
    )''')

    cursor.execute("DELETE FROM StudyLogs")
    cursor.execute("DELETE FROM LearningActivities")
    cursor.execute("DELETE FROM Exams")


    cursor.executemany('''
    INSERT INTO StudyLogs (UserId, CourseId, StudyDateUtc, DurationMinutes) 
    VALUES (?, ?, ?, ?)''', [
        (1, 101, '2026-07-20', 120), 
        (1, 101, '2026-07-21', 90),
        (1, 102, '2026-07-22', 60),  
        (1, 103, '2026-07-23', 180)  
    ])


    cursor.executemany('''
    INSERT INTO LearningActivities (UserId, CourseId, Title, Status) 
    VALUES (?, ?, ?, ?)''', [
        (1, 101, 'تمرین ۱ ریاضی', 'Completed'),
        (1, 101, 'تمرین ۲ ریاضی', 'Pending'),
        (1, 102, 'پروژه فاز ۱ دیتابیس', 'Completed')
    ])

    cursor.executemany('''
    INSERT INTO Exams (UserId, CourseId, Title, Grade) 
    VALUES (?, ?, ?, ?)''', [
        (1, 101, 'میان‌ترم ریاضی', 18.5),
        (1, 102, 'میان‌ترم دیتابیس', 15.0)
    ])

    conn.commit()
    conn.close()
    print("The local database was successfully created, and sample entries were added!")

if __name__ == '__main__':
    init_mock_db()