# Analytics Engine

## Overview

Analytics Engine is responsible for analyzing student learning data and generating dashboard metrics.

## Features

This module provides:

- Total study hours
- Average grade calculation
- Task completion rate
- Weekly study trend
- Course study distribution
- Task status analysis
- Grade trend analysis
- Study streak calculation
- AI learning insights


## Available Functions

- get_summary_cards()
- get_weekly_trend()
- get_course_distribution()
- get_task_status()
- get_grade_trend()
- get_study_streak()
- generate_ai_insights()
- get_dashboard_data()


## Output

The engine returns dashboard data as JSON.

The output can be used by the backend API and frontend charts.


Example:

{
    "summary_cards": {
        "total_study_hours": 7.5,
        "average_grade": 16.25,
        "task_completion_rate": 66.7
    }
}


## Database

Current development version uses SQLite.

Later it can be connected to the main SQL Server database through backend.


## Backend Integration

Backend can call:

engine.get_dashboard_data()

and return the generated JSON data through API.