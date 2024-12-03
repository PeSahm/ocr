from flask import Blueprint, request, jsonify, current_app
from ..utils.request_validators import validate_json_request

bp = Blueprint('ocr', __name__)

@bp.route('/api/v1/ocr', methods=['POST'])
@validate_json_request(['base64_string'])
def ocr_endpoint():
    data = request.get_json()
    result = current_app.image_processor.process_image(data['base64_string'])
    
    if "error" in result:
        return jsonify(result), 400
    
    return jsonify(result)