OS=darwin-10.6
RAMBLER_VERSION=5.4.0

rambler_v${RAMBLER_VERSION}:
	curl --compressed -#Lo rambler_v${RAMBLER_VERSION} \
		https://github.com/elwinar/rambler/releases/download/v${RAMBLER_VERSION}/rambler-${OS}-amd64

rambler: rambler_v${RAMBLER_VERSION}
	cp rambler_v${RAMBLER_VERSION} rambler
	chmod +x rambler

.PHONY: tool-restore
tool-restore:
	dotnet tool restore

.PHONY : migrations
migrations: rambler tool-restore
	./rambler apply --all
	dotnet sqlhydra-npgsql
	
.PHONY : reverse-migrations
reverse-migrations:
	./rambler reverse --all

.PHONY : bounce-migrations
bounce-migrations: reverse-migrations migrations

.PHONY: run-watch
run-watch:
	dotnet watch run

.PHONY: run
run:
	dotnet run
