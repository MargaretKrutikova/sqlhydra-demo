#!/bin/sh
until /rambler apply --all && dotnet SqlHydraDemo.dll; do
    echo "Crashed with exit code $?. Respawning in 5s.." >&2
    sleep 5
done