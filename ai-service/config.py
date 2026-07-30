import os
import pyodbc
from dotenv import load_dotenv

load_dotenv()

CONNECTION_STRING = os.getenv("CONNECTION_STRING")

if not CONNECTION_STRING:
    drivers = pyodbc.drivers()

    if "ODBC Driver 18 for SQL Server" in drivers:
        driver = "ODBC Driver 18 for SQL Server"
    elif "ODBC Driver 17 for SQL Server" in drivers:
        driver = "ODBC Driver 17 for SQL Server"
    else:
        driver = "SQL Server"

    CONNECTION_STRING = (
        f"DRIVER={{{driver}}};"
        "SERVER=.;"
        "DATABASE=StudentInsightsDb;"
        "Trusted_Connection=yes;"
        "TrustServerCertificate=yes;"
    )