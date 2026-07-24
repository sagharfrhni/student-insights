import os
import sqlite3
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
        """Fetch overall KPI metrics for user dashboard."""
        with get_db_connection() as conn:
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

        completion = 0
        if total_tasks:
            completion = round(completed_tasks * 100 / total_tasks, 1)

        return {
            "total_study_hours": round(total_hours, 1),
            "average_grade": round(avg_grade, 2),
            "task_completion_rate": completion,
        }

    def get_weekly_trend(self):
        """Get daily study hours for the last 7 active days."""
        with get_db_connection() as conn:
            rows = conn.execute(
                """
                SELECT 
                    DATE(StudyDateUtc) day, 
                    SUM(DurationMinutes)/60.0 hours
                FROM StudyLogs
                WHERE UserId=?
                GROUP BY DATE(StudyDateUtc)
                ORDER BY day DESC
                LIMIT 7
                """,
                (self.user_id,),
            ).fetchall()

        # Reverse to show chronological order
        return {
            "labels": [row["day"] for row in rows[::-1]],
            "datasets": [
                {
                    "label": "Study Hours",
                    "data": [round(row["hours"], 1) for row in rows[::-1]],
                }
            ],
        }

    def get_course_distribution(self):
        """Calculate total hours spent per course."""
        with get_db_connection() as conn:
            rows = conn.execute(
                """
                SELECT 
                    Courses.Name, 
                    SUM(StudyLogs.DurationMinutes)/60.0 hours
                FROM StudyLogs
                JOIN Courses ON Courses.Id = StudyLogs.CourseId
                WHERE StudyLogs.UserId=?
                GROUP BY Courses.Name
                """,
                (self.user_id,),
            ).fetchall()

        return {
            "labels": [r["Name"] for r in rows],
            "datasets": [
                {"data": [round(r["hours"], 1) for r in rows]}
            ],
        }

    def get_task_status(self):
        """Count activities grouped by status."""
        with get_db_connection() as conn:
            rows = conn.execute(
                """
                SELECT Status, COUNT(*) total
                FROM LearningActivities
                WHERE UserId=?
                GROUP BY Status
                """,
                (self.user_id,),
            ).fetchall()

        return {
            "labels": [r["Status"] for r in rows],
            "datasets": [{"data": [r["total"] for r in rows]}],
        }

    def get_grade_trend(self):
        """Fetch chronological exam grades."""
        with get_db_connection() as conn:
            rows = conn.execute(
                """
                SELECT ExamDateUtc, Grade
                FROM Exams
                WHERE UserId=?
                ORDER BY ExamDateUtc
                """,
                (self.user_id,),
            ).fetchall()

        return {
            "labels": [r["ExamDateUtc"][:10] for r in rows],
            "datasets": [
                {
                    "label": "Grade",
                    "data": [r["Grade"] for r in rows],
                }
            ],
        }

    def get_study_streak(self):
        """Calculate consecutive study days count."""
        with get_db_connection() as conn:
            rows = conn.execute(
                """
                SELECT DISTINCT DATE(StudyDateUtc) day
                FROM StudyLogs
                WHERE UserId=?
                ORDER BY day DESC
                """,
                (self.user_id,),
            ).fetchall()

        dates = [
            datetime.strptime(r["day"], "%Y-%m-%d") 
            for r in rows
        ]

        streak = 0
        if dates:
            today = dates[0]
            for i, date in enumerate(dates):
                if today - date == timedelta(days=i):
                    streak += 1
                else:
                    break

        return {"current_streak": streak}

    def generate_ai_insights(self):
        """Generate simple heuristic feedback based on KPIs."""
        cards = self.get_summary_cards()
        insights = []

        if cards["task_completion_rate"] < 60:
            insights.append("Your task completion rate needs improvement.")

        if cards["average_grade"] < 15:
            insights.append("Try increasing your study time to improve grades.")

        if cards["total_study_hours"] < 5:
            insights.append("Your weekly study hours are low.")

        if not insights:
            insights.append("Excellent progress. Keep going!")

        return insights

    def get_dashboard_data(self):
        """Aggregate all analytics for front-end consumption."""
        return {
            "summary_cards": self.get_summary_cards(),
            "charts": {
                "weekly_trend": self.get_weekly_trend(),
                "course_distribution": self.get_course_distribution(),
                "task_status": self.get_task_status(),
                "grade_trend": self.get_grade_trend(),
            },
            "analytics": {
                "study_streak": self.get_study_streak()
            },
            "ai_insights": self.generate_ai_insights(),
        }


if __name__ == "__main__":
    import json

    engine = AnalyticsEngine(1)
    result = engine.get_dashboard_data()
    print(json.dumps(result, ensure_ascii=False, indent=4))