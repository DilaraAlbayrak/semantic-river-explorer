import geopandas as gpd
from shapely.geometry.base import BaseGeometry
from shapely.validation import make_valid

def load_geopackage(path: str, layer: str) -> gpd.GeoDataFrame:
    """Reads a specific layer from a GeoPackage file."""
    return gpd.read_file(path, layer=layer)

def validate_geometry(geom: BaseGeometry) -> bool:
    """Returns True if geometry is valid."""
    return geom.is_valid

def ensure_valid_geometries(gdf: gpd.GeoDataFrame) -> gpd.GeoDataFrame:
    """Fixes invalid geometries using buffer(0)."""
    gdf["geometry"] = gdf["geometry"].apply(make_valid)
    return gdf