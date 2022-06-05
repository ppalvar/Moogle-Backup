from operator import ne
import os

files = os.listdir()

for file in files:
    if 'txt' not in file:
        continue
    
    new_name = file.split('.')[0]
    new_name = '_'.join(new_name.lower().split(' ')) + '.txt'
    os.rename(file, new_name)
