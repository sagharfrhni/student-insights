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

    def get_weekly_trend(self):
        conn = get_db_connection()

        rows = conn.execute(
            """
            SELECT
                DATE(StudyDateUtc) as day,
                SUM(DurationMinutes)/60.0 as hours
            FROM StudyLogs
            WHERE UserId=?
            GROUP BY DATE(StudyDateUtc)
            ORDER BY day
            LIMIT 7
        """,
            (self.user_id,),
        ).fetchall()

        conn.close()

        labels = []
        data = []

        for row in rows:
            labels.append(row["day"])
            data.append(round(row["hours"], 1))

        return {
            "labels": labels,
            "datasets": [
                {
                    "label": "Study Hours",
                    "data": data,
                }
            ],
        }

    def get_course_distribution(self):
        conn = get_db_connection()

        rows = conn.execute(
            """
            SELECT
                Courses.Name,
                SUM(StudyLogs.DurationMinutes)/60.0 as hours
            FROM StudyLogs
            JOIN Courses
                ON Courses.Id=StudyLogs.CourseId
            WHERE StudyLogs.UserId=?
            GROUP BY Courses.Name
        """,
            (self.user_id,),
        ).fetchall()

        conn.close()

        return {
            "labels": [r["Name"] for r in rows],
            "datasets": [
                {
                    "data": [round(r["hours"], 1) for r in rows],
                }
            ],
        }

    def get_task_status(self):
        conn = get_db_connection()

        rows = conn.execute(
            """
            SELECT
                Status,
                COUNT(*) as total
            FROM LearningActivities
            WHERE UserId=?
            GROUP BY Status
        """,
            (self.user_id,),
        ).fetchall()

        conn.close()

        return {
            "labels": [r["Status"] for r in rows],
            "datasets": [
                {
                    "data": [r["total"] for r in rows],
                }
            ],
        }

    def get_grade_trend(self):
        conn = get_db_connection()

        rows = conn.execute(
            """
            SELECT
                ExamDateUtc,
                Grade
            FROM Exams
            WHERE UserId=?
            ORDER BY ExamDateUtc
        """,
            (self.user_id,),
        ).fetchall()

        conn.close()

        return {
            "labels": [r["ExamDateUtc"][:10] for r in rows],
            "datasets": [
                {
                    "label": "Grade",
                    "data": [r["Grade"] for r in rows],
                }
            ],
        }

    def generate_ai_insights(self):
        cards = self.get_summary_cards()
        insights = []

        if cards["task_completion_rate"] < 60:
            insights.append("Complete your pending tasks before starting new ones.")
            
        if cards["average_grade"] < 15:
            insights.append("Your average grade is below target. Increase weekly study time.")
            
        if cards["total_study_hours"] < 5:
            insights.append("Study time is lower than recommended this week.")

        if not insights:
            insights.append("Excellent progress. Keep up the good work!")

        return insights

    def get_dashboard_data(self):
        return {
            "summary_cards": self.get_summary_cards(),
            "charts": {
                "weekly_trend": self.get_weekly_trend(),
                "course_distribution": self.get_course_distribution(),
                "task_status": self.get_task_status(),
                "grade_trend": self.get_grade_trend(),
            },
            "ai_insights": self.generate_ai_insights(),
        }