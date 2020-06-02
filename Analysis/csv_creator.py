import os

import pandas as pd
import glob


def load_csv():

    path = '../lasertag/src-gen/'
    all_filenames = [path+'Yellow.csv', path+'Green.csv', path+'Blue.csv', path+'Red.csv']

    map_csv = pd.read_csv(path+"Battleground.csv", delimiter=';', header=None)
    # export to csv
    map_csv.to_csv("map.csv", sep=';', index=False, encoding='utf-8-sig', header=False)

    li = []
    combined_csv = pd
    for f in all_filenames:
        try:
            # combine all files in the list
            df = pd.read_csv(f.strip(), index_col=None, delimiter=';', header=0)
            li.append(df)
        except pd.errors.EmptyDataError:
            print('Note: {} was empty. Skipping.'.format(f.replace(path, '')))
            continue  # will skip the rest of the block and move to next file

    # concat file content
    frame = pd.concat(li, axis=0, ignore_index=True)
    # sort the list
    frame.sort_values(by=['Tick', 'color'], inplace=True)
    # export to csv
    frame.to_csv("agents.csv", sep=';', index=False, encoding='utf-8-sig')

if __name__ == '__main__':
    load_csv()
