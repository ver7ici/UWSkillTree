import json
import requests

api = "https://openapi.data.uwaterloo.ca/v3/"
headers = {
    "x-api-key": "ADD534B3893E4F958CB96C49AC79B260",
}

def terms():
    response = requests.get(api + "terms", headers=headers)
    if response.status_code != 200:
        
        with open("terms.json", "w") as f:
            f.write(json.dumps(response.json(), indent=4))
    
def courses():
    response = requests.get(api + "courses/1205", headers=headers)
    if response.status_code == 200:
        with open("courses.json", "w") as f:
            f.write(json.dumps(response.json(), indent=4))
    else:
        print(response.status_code)

def schedule():
    response = requests.get(api + "classschedules/1205", headers=headers)
    if response.status_code != 200:
        print(response.status_code)
        return
    with open("schedule.json", "w") as f:
        f.write(json.dumps(response.json(), indent=4))
    
def courseDescription(term, subject, catalogNumber):
    response = requests.get("{}courses/{}/{}/{}".format(api, term, subject, catalogNumber), headers=headers)
    if response.status_code != 200:
        print(response.status_code)
        return
    d = response.json()[0]
    return d["description"] + "\n\n" + d["requirementsDescription"]


def main():
    print(courseDescription("1235", "cs", "240"))

if __name__ == "__main__":
    main()
