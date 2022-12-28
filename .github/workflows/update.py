def main():
  print("Hello world")
  with open("wwwroot/sample-data/courses", "w") as f:
	f.write("hello world")
  
if __name__ == "__main__":
  main()
