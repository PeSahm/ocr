from typing import Dict, Any
from fastapi import HTTPException

def validate_request_data(data: Dict[str, Any], required_fields: list) -> None:
    """Validate request data for required fields."""
    missing_fields = [field for field in required_fields if field not in data]
    if missing_fields:
        raise HTTPException(
            status_code=400,
            detail=f"Missing required fields: {', '.join(missing_fields)}"
        )