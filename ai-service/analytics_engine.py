import sqlite3
import os
from datetime import datetime, timedelta

def get_db_connection():
    current_dir = os.path.dirname(os.path.abspath(__file__))
    db_path = os.path.join(current_dir, "local_test.db")

    conn = sqlite3.connect(db_path)
    conn.row_factory = sqlite3.Row
    return conn

class AnalyticsEngine:

    def __init__(self, user_id=1):
        self.user_id = user_id

    def get_summary_cards(self):
        conn = get_db_connection()

        total_hours = conn.execute(
            """
            SELECT IFNULL(SUM(DurationMinutes),0)/60.0
            FROM StudyLogs
            WHERE UserId=?
        """,
            (self.user_id,),
        ).fetchone()[0]

        avg_grade = conn.execute(
            """
            SELECT IFNULL(AVG(Grade),0)
            FROM Exams
            WHERE UserId=?
        """,
            (self.user_id,),
        ).fetchone()[0]

        total_tasks = conn.execute(
            """
            SELECT COUNT(*)
            FROM LearningActivities
            WHERE UserId=?
        """,
            (self.user_id,),
        ).fetchone()[0]

        completed_tasks = conn.execute(
            """
            SELECT COUNT(*)
            FROM LearningActivities
            WHERE UserId=?
            AND Status='Completed'
        """,
            (self.user_id,),
        ).fetchone()[0]

        conn.close()

        completion = 0

        if total_tasks:
            completion = round(completed_tasks * 100 / total_tasks, 1)

        return {
            "total_study_hours": round(total_hours, 1),
            "average_grade": round(avg_grade, 2),
            "task_completion_rate": completion,
        }