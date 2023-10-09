"""execute from root of project
"""

from datetime import datetime
import json
import os
import re
import requests

api = "https://openapi.data.uwaterloo.ca/v3/"
headers = {
    "x-api-key": "ADD534B3893E4F958CB96C49AC79B260",
}

# -------------------------------------------------

def tokenize(s):
    tkns = list()
    i = 0
    j = 0
    while j < len(s):
        if re.findall("\W", s[j]):
            if s[i:j]:
                tkns.append(s[i:j])
            i = j + 1
        j += 1
    if s[i:]:
        tkns.append(s[i:])
    return tkns

def isCN(s):
    if re.findall("\A[0-9]{3}[A-Z]?\Z", s):
        return True
    return False

def isSubject(s):
    if re.findall("\A[A-Z]+\Z", s):
        return True
    return False

def parseReq(reqTkns):
    courses = list()
    groups = " ".join(reqTkns).split(";")
    for group in groups:
        tkns = tokenize(group)
        subject = list()
        for i in range(len(tkns)):
            if isSubject(tkns[i]):
                if i > 0 and not isSubject(tkns[i-1]):
                    subject = list()
                subject.append(tkns[i])
            elif isCN(tkns[i]) and subject:
                for s in subject:
                    courses.append(s + " " + tkns[i])
 
    return courses
    
def parseReqDesc(reqDesc):
    req = {
        "prereq": [],
        "coreq": [],
        "antireq": [],
    }
    
    if not reqDesc:
        return req
    
    tkns = reqDesc.split()
    j = len(tkns)
    for i in reversed(range(len(tkns) - 1)):
        if "req" not in tkns[i].lower():
            continue
        
        if "pre" in tkns[i].lower():
            req["prereq"] = parseReq(tkns[i:j])
            j = i
        elif "co" in tkns[i].lower():
            req["coreq"] = parseReq(tkns[i:j])
            j = i
        elif "anti" in tkns[i].replace(":", "").lower():
            req["antireq"] = parseReq(tkns[i:j])
            j = i
        
    return req
    
def getCourses(term):
    response = requests.get(api + "courses/" + term, headers=headers)
    if response.status_code != 200:
        return None
    
    courses = dict()
    for c in response.json():
        req = parseReqDesc(c["requirementsDescription"])
        
        courses.update({c["subjectCode"] + " " + c["catalogNumber"]: {
            "id": c["courseId"],
            "subject": c["subjectCode"],
            "catalogNumber": c["catalogNumber"],
            "title": c["title"],
            "description": c["description"],
            "term": [],
            "prereq": req["prereq"],
            "coreq": req["coreq"],
            "antireq": req["antireq"],
            "next": [],
        }})
        
    return courses

def getCurrentSeason():
    now = datetime.now()
    y = now.year
    m = now.month
    
    s = ""
    if m <= 4:
        s = "Winter"
    elif m <= 8:
        s = "Spring"
    else:
        s = "Fall"
    
    return (s, y)
    
def getTerms():
    response = requests.get(api + "terms", headers=headers)
    if response.status_code != 200:
        return None
    
    seasons = [
        tuple(),
        getCurrentSeason(),
        tuple(),
    ]
    if seasons[1][0] == "Winter":
        seasons[0] = ("Fall", seasons[1][1] - 1)
        seasons[2] = ("Spring", seasons[1][1])
    elif seasons[1][0] == "Spring":
        seasons[0] = ("Winter", seasons[1][1])
        seasons[2] = ("Fall", seasons[1][1])
    else:
        seasons[0] = ("Spring", seasons[1][1])
        seasons[2] = ("Winter", seasons[1][1] + 1)
    
    terms = list()
    for s in seasons:
        for t in response.json():
            if t["name"] == "%s %d" % s:
                terms.append({
                    "code": t["termCode"],
                    "season": s[0],
                    "year": s[1],
                })
                break
    
    return terms

def getSchedule(term):
    response = requests.get(api + "classschedules/" + term, headers=headers)
    if response.status_code != 200:
        return None
    
    return response.json()
      
# -------------------------------------------------

def main():
    print("retrieving terms ... ", end="", flush=True)
    terms = getTerms()
    print("done")
    
    
    print("retrieving courses ... ", end="", flush=True)
    courses = dict()
    for t in terms:
        courses.update(getCourses(t["code"]))
    print("done")
    
    names = [None] * 1000000
    for k, v in courses.items():
        names[int(v["id"])] = k
    
    for t in terms:
        schedule = getSchedule(t["code"])
        for i in [int(s) for s in schedule]:
            if names[i]:
                courses[names[i]]["term"].append(t["season"])
         
    for k1 in courses:
        for k2 in courses:
            if k1 in courses[k2]["prereq"]:
                courses[k1]["next"].append(k2)
    
    path = "wwwroot/data"
    print("creating index at %s ... " % path, end="", flush=True)
    with open("{}/courses.json".format(path), "w") as f:
        f.write(json.dumps(courses, indent=4))
    print("done")
  
if __name__ == "__main__":
    main()
