from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from analytics_engine import AnalyticsEngine

app = FastAPI(
    title="Student Insights Analytics API",
    version="1.0.0"
)

# React / Frontend
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],   
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/")
def home():
    return {
        "message": "Analytics Service is running."
    }


@app.get("/api/dashboard/{user_id}")
def dashboard(user_id: int):

    engine = AnalyticsEngine(user_id)

    return engine.get_dashboard_data()


@app.get("/api/summary/{user_id}")
def summary(user_id: int):

    engine = AnalyticsEngine(user_id)

    return engine.get_summary_cards()


@app.get("/api/insights/{user_id}")
def insights(user_id: int):

    engine = AnalyticsEngine(user_id)

    return engine.generate_ai_insights()