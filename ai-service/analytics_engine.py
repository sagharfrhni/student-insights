import sqlite3
import os
from datetime import datetime, timedelta

def get_db_connection():
    current_dir = os.path.dirname(os.path.abspath(__file__))
    db_path = os.path.join(current_dir, "local_test.db")

    conn = sqlite3.connect(db_path)
    conn.row_factory = sqlite3.Row
    return conn