import pathlib
import requests
import json
from bs4 import BeautifulSoup
import datetime
import re



def get_page() -> str:
    """ get from school page """

    _url = "https://www.yzu.edu.tw/admin/aa/index.php/tw/2016-01-14-06-58-46/2016-03-08-03-41-34/2016-03-08-03-50-32"
    _response = requests.get(_url)

    return _response.text

def load_last_updated_date() -> datetime.date:

    _file_path = file_folder / "version.json"
    with open( _file_path , mode='r' ) as file:
        _version_json = json.load(file)

    _data_string = _version_json["CourseSchedule"]
    return datetime.date.fromisoformat(_data_string)

def get_lastest_updated_date( html_string: str ) -> datetime.date:
    
    _soup = BeautifulSoup(html_string, 'html.parser')
    _found = _soup.select(".article-info > .modified > time")[0].get_text()

    _data_re = re.compile(r'\d.*\d')
    _match = _data_re.search(_found)
    if _match is None:
        raise RuntimeError("failed to find lastest updated date")
    else:
        _data_string = _match.group(0)
        return datetime.date.fromisoformat(_data_string)
        
        
# create folder for file 
file_folder = pathlib.Path("./data/class-viewer/")
file_folder.mkdir( parents=True , exist_ok=True )

html_string = get_page()

old_updated_date = load_last_updated_date()
lastest_updated_date = get_lastest_updated_date( html_string )

if lastest_updated_date > old_updated_date:

    print("found Schedule update")
    print(f"old_updated_date -> {old_updated_date}")
    print(f"lastest_updated_date -> {lastest_updated_date}")

    raise RuntimeError("found Schedule update")

else:

    print("not found Schedule update")
    print(f"old_updated_date -> {old_updated_date}")
    print(f"lastest_updated_date -> {lastest_updated_date}")


