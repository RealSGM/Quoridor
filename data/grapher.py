import json
import matplotlib.pyplot as plt
import os

data = {}
FILE_NAME = 'data/algorithm_data.json'
SAVE_FILE = 'data/processed_data.json'
GRAPH_DIR = 'data/graphs'

processed_data = {}

# Ensure graph directory exists
os.makedirs(GRAPH_DIR, exist_ok=True)

metric_labels = {
    'win_rate': 'Win Rate',
}

# Store per-metric data to graph after parsing all
metric_lines = {
    'Fences Remaining': {},
    'Nodes': {},
    'Pawn Moves': {},
    'Move Speeds': {}
}

def graph_single_metric_bar(processed_data: dict, metric: str, output_dir='data/graphs'):
    # Capitalize and format the metric name for the title
    formatted_name = metric.replace('_', ' ').title()
    
    # Extract algorithm names and metric values
    algorithms = list(processed_data.keys())
    values = [processed_data[algo].get(metric, 0) for algo in algorithms]

    # Plot bar graph
    plt.figure(figsize=(10, 6))
    bars = plt.bar(algorithms, values, color='skyblue', edgecolor='black')
    
    # Annotate bars with values
    for bar, value in zip(bars, values):
        height = bar.get_height()
        plt.text(bar.get_x() + bar.get_width() / 2, height, f'{value:.2f}',
            ha='center', va='bottom', fontsize=10)

    plt.title(f'{formatted_name} by Algorithm')
    plt.xlabel('Algorithm')
    plt.ylabel(formatted_name)
    plt.grid(axis='y', linestyle='--', alpha=0.6)

    os.makedirs(output_dir, exist_ok=True)
    filename = f'{output_dir}/{metric}.png'
    plt.tight_layout()
    plt.savefig(filename)
    plt.close()
    print(f'Bar graph saved to: {filename}')

def graph_data_all_algorithms(metric_data: dict, metric_name: str):
    plt.figure(figsize=(10, 6))
    
    for algo_name, values in metric_data.items():
        turns = list(range(1, len(values) + 1))
        plt.plot(turns, values, marker='o', linestyle='-', label=algo_name)
    
    plt.title(f'Average {metric_name} As Game Progresses')
    plt.xlabel('Round Number')
    plt.ylabel(f'Average {metric_name}')
    plt.grid(True)
    plt.legend()
    
    filename = f"{GRAPH_DIR}/{metric_name.replace(' ', '_').lower()}.png"
    plt.savefig(filename)
    plt.close()
    print(f"Plot saved to: {filename}")

def save_data():
    with open(SAVE_FILE, 'w') as f:
        json.dump(processed_data, f, indent=4)
    print(f"Processed data saved to {SAVE_FILE}")

def load_data():
    global data
    if not os.path.exists(FILE_NAME) or os.stat(FILE_NAME).st_size == 0:
        print(f"File {FILE_NAME} is missing or empty.")
        return
    with open(FILE_NAME, 'r') as f:
        data = json.load(f)

def parse_data(key: str):
    if key not in data:
        return None
    print(f"Parsing data for {key}...")

    d = data[key]
    
    wins = float(d['wins'])
    games_played = float(d['games_played'])
    fences_cumulative = d['fences_remaining_cumulative']
    moves_cumulative = d['moves_made_cumulative']
    move_speeds_cumulative = d['move_speeds_cumulative']
    nodes_cumulative = d['nodes_searched_cumulative']
    pawn_moves_cumulative = d['pawn_moves_cumulative']
    
    total_moves = sum(moves_cumulative)
    total_move_speed = sum(move_speeds_cumulative)
    total_nodes = sum(nodes_cumulative)
    total_pawn_moves = sum(pawn_moves_cumulative)

    average_win_rate = wins / games_played if games_played > 0 else 0
    average_move_speed = total_move_speed / total_moves if total_moves > 0 else 0
    average_nodes_total = total_nodes / games_played if games_played > 0 else 0
    average_pawn_moves_total = total_pawn_moves / games_played if games_played > 0 else 0
    average_game_length = total_moves / games_played if games_played > 0 else 0

    # Per-turn averages
    average_fences_remaining = [
        round(f / m, 2) if m > 0 else 0
        for f, m in zip(fences_cumulative, moves_cumulative)
    ]
    
    average_nodes = [
        round(n / m, 2) if m > 0 else 0
        for n, m in zip(nodes_cumulative, moves_cumulative)
    ]
    
    average_pawn_moves = [
        round(p / m, 2) if m > 0 else 0
        for p, m in zip(pawn_moves_cumulative, moves_cumulative)
    ]
    
    average_speeds = [
        round(s / m, 2) if m > 0 else 0
        for s, m in zip(move_speeds_cumulative, moves_cumulative)
    ]
    
    # Store for final output
    processed_data[key] = {
        'win_rate': average_win_rate,
        'move_speed': average_move_speed,
        'nodes': average_nodes_total,
        'pawn_moves': average_pawn_moves_total,
        'game_length': average_game_length
    }

    # Store for global graphing
    metric_lines['Fences Remaining'][key] = average_fences_remaining
    metric_lines['Nodes'][key] = average_nodes
    metric_lines['Pawn Moves'][key] = average_pawn_moves
    metric_lines['Move Speeds'][key] = average_speeds

if __name__ == '__main__':
    load_data()
    for key in data:
        parse_data(key)

    # Plot each metric with all algorithms
    for metric_name, algo_data in metric_lines.items():
        graph_data_all_algorithms(algo_data, metric_name)
    
    graph_single_metric_bar(processed_data, 'win_rate')
    graph_single_metric_bar(processed_data, 'game_length')

    save_data()
