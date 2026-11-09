# Evidências de funcionamento

Esta pasta reúne respostas JSON obtidas ao executar a API e chamar seus endpoints. O arquivo da jornada redige token, link, coordenadas e horários exatos; os arquivos do SOS não contêm o PIN.

`swagger.json` registra o OpenAPI publicado pela aplicação. `report-workflow.json` mostra a criação, confirmação e resolução de um relato. Os outros arquivos documentam health check, relatos ativos, comparação de rotas, localização compartilhada, etapas do SOS e resumo do painel municipal.

Para reproduzir as chamadas, execute a API em modo Development e rode `scripts/gerar-evidencias.ps1` em outro terminal. O script usa `http://localhost:5080` por padrão e aceita a URL base como parâmetro.
