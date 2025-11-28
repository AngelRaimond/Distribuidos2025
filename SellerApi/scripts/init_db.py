import boto3
import os
from botocore.exceptions import ClientError
from dotenv import load_dotenv

load_dotenv()


def create_sellers_table():
    """Create the Sellers table in DynamoDB with required indexes"""
    
    dynamodb = boto3.resource(
        'dynamodb',
        endpoint_url=os.getenv('DYNAMODB_ENDPOINT', 'http://localhost:8000'),
        region_name=os.getenv('AWS_REGION', 'us-east-1'),
        aws_access_key_id=os.getenv('AWS_ACCESS_KEY_ID', 'test'),
        aws_secret_access_key=os.getenv('AWS_SECRET_ACCESS_KEY', 'test')
    )
    
    table_name = os.getenv('DYNAMODB_TABLE_NAME', 'Sellers')
    
    try:
        # Check if table already exists
        existing_tables = dynamodb.meta.client.list_tables()['TableNames']
        if table_name in existing_tables:
            print(f"Table {table_name} already exists")
            return
        
        # Create table
        table = dynamodb.create_table(
            TableName=table_name,
            KeySchema=[
                {
                    'AttributeName': 'id',
                    'KeyType': 'HASH'  # Partition key
                }
            ],
            AttributeDefinitions=[
                {
                    'AttributeName': 'id',
                    'AttributeType': 'S'  # String
                },
                {
                    'AttributeName': 'email',
                    'AttributeType': 'S'  # String
                },
                {
                    'AttributeName': 'name',
                    'AttributeType': 'S'  # String
                }
            ],
            GlobalSecondaryIndexes=[
                {
                    'IndexName': 'EmailIndex',
                    'KeySchema': [
                        {
                            'AttributeName': 'email',
                            'KeyType': 'HASH'
                        }
                    ],
                    'Projection': {
                        'ProjectionType': 'ALL'
                    },
                    'ProvisionedThroughput': {
                        'ReadCapacityUnits': 5,
                        'WriteCapacityUnits': 5
                    }
                },
                {
                    'IndexName': 'NameIndex',
                    'KeySchema': [
                        {
                            'AttributeName': 'name',
                            'KeyType': 'HASH'
                        }
                    ],
                    'Projection': {
                        'ProjectionType': 'ALL'
                    },
                    'ProvisionedThroughput': {
                        'ReadCapacityUnits': 5,
                        'WriteCapacityUnits': 5
                    }
                }
            ],
            ProvisionedThroughput={
                'ReadCapacityUnits': 5,
                'WriteCapacityUnits': 5
            }
        )
        
        # Wait for table to be created
        print(f"Creating table {table_name}...")
        table.wait_until_exists()
        print(f"Table {table_name} created successfully!")
        
        # Print table description
        print("\nTable details:")
        print(f"  Table name: {table.table_name}")
        print(f"  Table status: {table.table_status}")
        print(f"  Item count: {table.item_count}")
        
    except ClientError as e:
        print(f"Error creating table: {e.response['Error']['Message']}")
        raise
    except Exception as e:
        print(f"Unexpected error: {str(e)}")
        raise


def delete_sellers_table():
    """Delete the Sellers table"""
    
    dynamodb = boto3.resource(
        'dynamodb',
        endpoint_url=os.getenv('DYNAMODB_ENDPOINT', 'http://localhost:8000'),
        region_name=os.getenv('AWS_REGION', 'us-east-1'),
        aws_access_key_id=os.getenv('AWS_ACCESS_KEY_ID', 'test'),
        aws_secret_access_key=os.getenv('AWS_SECRET_ACCESS_KEY', 'test')
    )
    
    table_name = os.getenv('DYNAMODB_TABLE_NAME', 'Sellers')
    
    try:
        table = dynamodb.Table(table_name)
        table.delete()
        print(f"Table {table_name} deleted successfully!")
    except ClientError as e:
        print(f"Error deleting table: {e.response['Error']['Message']}")
        raise


def list_tables():
    """List all DynamoDB tables"""
    
    dynamodb = boto3.client(
        'dynamodb',
        endpoint_url=os.getenv('DYNAMODB_ENDPOINT', 'http://localhost:8000'),
        region_name=os.getenv('AWS_REGION', 'us-east-1'),
        aws_access_key_id=os.getenv('AWS_ACCESS_KEY_ID', 'test'),
        aws_secret_access_key=os.getenv('AWS_SECRET_ACCESS_KEY', 'test')
    )
    
    try:
        response = dynamodb.list_tables()
        tables = response.get('TableNames', [])
        
        if tables:
            print("Existing DynamoDB tables:")
            for table in tables:
                print(f"  - {table}")
        else:
            print("No tables found")
            
    except ClientError as e:
        print(f"Error listing tables: {e.response['Error']['Message']}")
        raise


if __name__ == '__main__':
    import sys
    
    if len(sys.argv) > 1:
        command = sys.argv[1]
        
        if command == 'create':
            create_sellers_table()
        elif command == 'delete':
            delete_sellers_table()
        elif command == 'list':
            list_tables()
        else:
            print("Usage: python init_db.py [create|delete|list]")
    else:
        # Default: create table
        create_sellers_table()
