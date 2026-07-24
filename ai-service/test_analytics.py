from analytics_engine import AnalyticsEngine


engine = AnalyticsEngine(1)


print(engine.get_summary_cards())

print(engine.get_weekly_trend())

print(engine.get_study_streak())

print("Analytics Engine works!")