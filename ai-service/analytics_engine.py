from datetime import datetime, timedelta
import pyodbc
from config import CONNECTION_STRING


def get_db_connection():
    return pyodbc.connect(CONNECTION_STRING)


def fetch_all(query, params=()):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute(query, params)
    columns = [column[0] for column in cursor.description]
    rows = [dict(zip(columns, row)) for row in cursor.fetchall()]
    conn.close()
    return rows


def fetch_one(query, params=()):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute(query, params)
    row = cursor.fetchone()
    if row is None:
        conn.close()
        return None
    columns = [column[0] for column in cursor.description]
    result = dict(zip(columns, row))
    conn.close()
    return result


class AnalyticsEngine:

    def __init__(self, user_id=1):
        self.user_id = user_id

    def get_summary_cards(self):
        """Fetch overall KPI metrics for user dashboard."""
        hours_res = fetch_one(
            """
            SELECT ISNULL(SUM(DurationMinutes), 0) / 60.0 AS total
            FROM StudyLogs
            WHERE UserId=?
            """,
            (self.user_id,),
        )
        total_hours = hours_res["total"] if hours_res else 0

        grade_res = fetch_one(
            """
            SELECT ISNULL(AVG(Grade), 0) AS total
            FROM Exams
            WHERE UserId=?
            """,
            (self.user_id,),
        )
        avg_grade = grade_res["total"] if grade_res else 0

        tasks_res = fetch_one(
            """
            SELECT COUNT(*) AS total
            FROM LearningActivities
            WHERE UserId=?
            """,
            (self.user_id,),
        )
        total_tasks = tasks_res["total"] if tasks_res else 0

        completed_res = fetch_one(
            """
            SELECT COUNT(*) AS total
            FROM LearningActivities
            WHERE UserId=?
            AND Status='Completed'
            """,
            (self.user_id,),
        )
        completed_tasks = completed_res["total"] if completed_res else 0

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
        rows = fetch_all(
            """
            SELECT TOP 7
                CAST(StudyDateUtc AS DATE) AS day, 
                SUM(DurationMinutes)/60.0 AS hours
            FROM StudyLogs
            WHERE UserId=?
            GROUP BY CAST(StudyDateUtc AS DATE)
            ORDER BY day DESC
            """,
            (self.user_id,),
        )

        # Reverse to show chronological order
        return {
            "labels": [str(row["day"]) for row in rows[::-1]],
            "datasets": [
                {
                    "label": "Study Hours",
                    "data": [round(row["hours"], 1) for row in rows[::-1]],
                }
            ],
        }

    def get_course_distribution(self):
        """Calculate total hours spent per course."""
        rows = fetch_all(
            """
            SELECT 
                Courses.Name, 
                SUM(StudyLogs.DurationMinutes)/60.0 AS hours
            FROM StudyLogs
            JOIN Courses ON Courses.Id = StudyLogs.CourseId
            WHERE StudyLogs.UserId=?
            GROUP BY Courses.Name
            """,
            (self.user_id,),
        )

        return {
            "labels": [r["Name"] for r in rows],
            "datasets": [{"data": [round(r["hours"], 1) for r in rows]}],
        }

    def get_task_status(self):
        """Count activities grouped by status."""
        rows = fetch_all(
            """
            SELECT Status, COUNT(*) AS total
            FROM LearningActivities
            WHERE UserId=?
            GROUP BY Status
            """,
            (self.user_id,),
        )

        return {
            "labels": [r["Status"] for r in rows],
            "datasets": [{"data": [r["total"] for r in rows]}],
        }

    def get_grade_trend(self):
        """Fetch chronological exam grades."""
        rows = fetch_all(
            """
            SELECT ExamDateUtc, Grade
            FROM Exams
            WHERE UserId=?
            ORDER BY ExamDateUtc
            """,
            (self.user_id,),
        )

        return {
            "labels": [str(r["ExamDateUtc"])[:10] for r in rows],
            "datasets": [
                {
                    "label": "Grade",
                    "data": [r["Grade"] for r in rows],
                }
            ],
        }

    def get_study_streak(self):
        """Calculate consecutive study days count."""
        rows = fetch_all(
            """
            SELECT DISTINCT CAST(StudyDateUtc AS DATE) AS day
            FROM StudyLogs
            WHERE UserId=?
            ORDER BY day DESC
            """,
            (self.user_id,),
        )

        dates = [
            datetime.strptime(str(r["day"])[:10], "%Y-%m-%d") for r in rows
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
        """Generate heuristic feedback based on KPIs and study streak."""
        cards = self.get_summary_cards()
        streak = self.get_study_streak()["current_streak"]

        insights = []

        if cards["task_completion_rate"] < 60:
            insights.append(
                f"Task completion rate is {cards['task_completion_rate']}%. Try finishing pending activities."
            )

        if cards["average_grade"] < 15:
            insights.append(
                "Average grade is below target. Increasing study time may improve performance."
            )

        if cards["total_study_hours"] < 5:
            insights.append(
                "Weekly study time is lower than recommended."
            )

        if streak >= 5:
            insights.append(
                f"Great consistency! Current study streak: {streak} days."
            )

        if not insights:
            insights.append(
                "Excellent progress. Keep up the good work!"
            )

        return insights

    def get_dashboard_data(self):
        """Aggregate all analytics for front-end consumption."""
        return {
            "generated_at": datetime.now().isoformat(),
            "summary_cards": self.get_summary_cards(),
            "charts": {
                "weekly_trend": self.get_weekly_trend(),
                "course_distribution": self.get_course_distribution(),
                "task_status": self.get_task_status(),
                "grade_trend": self.get_grade_trend(),
            },
            "analytics": {"study_streak": self.get_study_streak()},
            "ai_insights": self.generate_ai_insights(),
        }


if __name__ == "__main__":
    import json

    engine = AnalyticsEngine(1)
    result = engine.get_dashboard_data()
    print(json.dumps(result, ensure_ascii=False, indent=4))