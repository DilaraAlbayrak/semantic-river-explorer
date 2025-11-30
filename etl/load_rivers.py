import geopandas as gpd
from sqlalchemy import create_engine
from geoalchemy2 import Geometry
from config import (
    DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD,
    GPKG_PATH, GPKG_LAYER, TABLE_NAME
)
from utils import load_geopackage, ensure_valid_geometries


def build_connection_string() -> str:
    """Builds SQLAlchemy PostgreSQL/PostGIS connection string."""
    return (
        f"postgresql+psycopg2://{DB_USER}:{DB_PASSWORD}"
        f"@{DB_HOST}:{DB_PORT}/{DB_NAME}"
    )


def run_etl():
    print("Reading GeoPackage data...")
    gdf = load_geopackage(GPKG_PATH, GPKG_LAYER)

    print("Validating geometries...")
    gdf = ensure_valid_geometries(gdf)

    print("Loading data into PostGIS...")

    # Geometry column name
    geom_col = gdf.geometry.name

    engine = create_engine(build_connection_string())

    gdf.to_postgis(
        TABLE_NAME,
        engine,
        if_exists="replace",
        index=False,
        dtype={geom_col: Geometry("LINESTRING", srid=27700)}
    )

    print("ETL completed successfully.")


if __name__ == "__main__":
    run_etl()