import psycopg2
from sentence_transformers import SentenceTransformer
import time
from config import DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD

def get_connection():
    return psycopg2.connect(
        host=DB_HOST, port=DB_PORT, dbname=DB_NAME, user=DB_USER, password=DB_PASSWORD
    )

def run_semantic_etl():
    print("Connecting to database...")
    conn = get_connection()
    conn.autocommit = True 
    cur = conn.cursor()

    # Clean existing data
    # This ensures no duplicate records exist if the script is run multiple times.
    print("Cleaning existing semantic data (TRUNCATE)...")
    cur.execute("TRUNCATE TABLE river_semantic_index RESTART IDENTITY;")
    
    # Load local AI model
    # Using 'all-MiniLM-L6-v2' (Free & Open Source)
    print("Loading local AI model (all-MiniLM-L6-v2)...")
    model = SentenceTransformer('all-MiniLM-L6-v2')

    # Fetch data
    # Increased limit to 1000 for a better AI experience
    print("Fetching river data...")
    cur.execute("""
        SELECT id, watercourse_name, form, length 
        FROM rivers 
        WHERE watercourse_name IS NOT NULL 
        LIMIT 1000; 
    """)
    rows = cur.fetchall()

    print(f"Found {len(rows)} rivers. Generating embeddings...")

    count = 0
    for row in rows:
        r_id, name, form, length = row
        
        # Construct the semantic description
        description = f"River {name}. Form: {form}. Length: {length} meters."

        try:
            # Generate Vector (returns numpy array, convert to list)
            embedding_vector = model.encode(description).tolist()

            # Insert into database
            cur.execute(
                """
                INSERT INTO river_semantic_index (river_id, description, embedding)
                VALUES (%s, %s, %s)
                """,
                (r_id, description, embedding_vector)
            )
            
            count += 1
            if count % 50 == 0: 
                print(f"Processed {count} rivers...")

        except Exception as e:
            print(f"Error processing {name}: {e}")

    # Explicit commit (though autocommit is on, good practice to be explicit)
    # Note: TRUNCATE cannot be rolled back easily in some modes, but INSERTs can.
    # Since we set autocommit=True at the start, each statement is committed immediately.
    
    cur.close()
    conn.close()
    print("Semantic ETL completed successfully.")

if __name__ == "__main__":
    run_semantic_etl()