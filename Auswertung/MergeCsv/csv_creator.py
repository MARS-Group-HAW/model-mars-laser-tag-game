import os

import pandas as pd
import glob


def load_csv():
    #os.chdir(r'/Users/danielosterholz')

    extension = 'csv'
    all_filenames = [i for i in glob.glob('*.{}'.format(extension))]

    # combine all files in the list
    combined_csv = pd.concat([pd.read_csv(f.strip(), delimiter=';') for f in all_filenames])
    # sort the list
    combined_csv.sort_values(by=['Tick'], inplace=True)
    # print(combined_csv.head())
    # export to csv
    combined_csv.to_csv("agents.csv", sep=';', index=False, encoding='utf-8-sig')
    # print(combined_csv)

if __name__ == '__main__':
    load_csv()
