dotnet = dotnet
source_code_root = src
solution_file = ${source_code_root}/MinimalBlockchain.sln
api_proj = ${source_code_root}/MinimalBlockchain.Api/MinimalBlockchain.Api.csproj

.PHONY = run build clean

run:
	@${dotnet} run --project ${api_proj}

build:
	@${dotnet} build ${solution_file}

clean:
	@${dotnet} clean ${solution_file}