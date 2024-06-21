# Weather Data Aggregation

This project is a weather data aggregation application built with C#. It fetches weather data from the Open Meteo API and displays it in various formats. The application makes use of asynchronous programming to ensure a smooth and responsive user experience.

## Features

- Fetches hourly and daily weather data asynchronously
- Can display data from 1945-today at once
- Fetches weather data from the Open Meteo API
- Displays  data in livecharts2 plots
- Allows searching for locations and adding them to a collection for weather data fetching
- Automatically reloads data when the selected range or the selected locations change
- Displays weather forecast data for the next 7 days
- Working WebAssembly version

## Asynchronous Programming

The application uses Tasks for any significant operations, so that it stays responsive on user input. Additionally calls to the Open-Meteo API are parallellised for each location to speed up loading with multiple selected locations.
To update the UI, `Dispatcher.UIThread.InvokeAsync` is used to ensure that the UI is updated on the main thread.


### Requirements

- .NET 8.0
- Avalonia
- LiveCharts2
