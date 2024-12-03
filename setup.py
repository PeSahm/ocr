from setuptools import setup

setup(
    name="ocr-api",
    version="1.0.0",
    packages=["app"],
    install_requires=[
        "fastapi==0.104.1",
        "uvicorn==0.24.0",
        "python-multipart==0.0.6",
        "easyocr==1.7.0",
        "torch==2.0.1+cpu",
        "torchvision==0.15.2+cpu",
        "python-dotenv==1.0.0",
        "pydantic==1.10.13"
    ],
    python_requires=">=3.9",
)