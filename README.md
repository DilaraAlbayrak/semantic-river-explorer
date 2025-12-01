# Semantic River Explorer: AI-Powered Geospatial Agent
A hybrid .NET & Python microservices architecture for semantic geospatial search on the UK river network (based on [OS Open Rivers data](https://osdatahub.os.uk/data/downloads/open/OpenRivers)). By integrating a .NET 8 Backend with a Python AI Microservice, the system allows users to query the GIS database using natural language (e.g., "tidal rivers near the coast" or "large inland watercourses"). The system understands the intent and retrieves topologically relevant river segments.

## System Architecture
The system follows a Clean Architecture pattern, separating concerns between the AI logic, data access, and API presentation layers. It employs a Service-to-Service communication pattern (HTTP/REST) to bridge the .NET and Python environments. 
- Frontend sends the query to the .NET API.
- .NET Service forwards the text to the Python Microservice via HTTP. The .NET backend is structured to ensure modularity and testability, adhering to SOLID principles.
- Python AI generates a 384-dimensional vector embedding and returns it.
- .NET Service performs Cosine Similarity calculation against cached river vectors.
- PostGIS retrieves the geometry for the top matches.

<img width="2000" height="1185" alt="collage" src="https://github.com/user-attachments/assets/28220473-e976-46a7-aada-63d5aacc1b6c" />
<img width="1844" height="1190" alt="sequence" src="https://github.com/user-attachments/assets/143b6d03-177f-4451-ad31-e14511e51a9b" />

## Prerequisites
- PostgreSQL 15+ (with PostGIS extension enabled)
- .NET 8 SDK
- Python 3.9+

We create a database named `rivers_db` and enable the extensions. Use etl/load_rivers.py to populate the initial GIS data from the OS Open Rivers .gpkg file under "data" folder. Then, we install dependencies (`etl/requirements.txt`) and start the microservice. This service must remain running.
