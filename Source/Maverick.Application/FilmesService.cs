using Maverick.Domain.Adapters;
using Maverick.Domain.Exceptions;
using Maverick.Domain.Models;
using Maverick.Domain.Services;
using Microsoft.Extensions.Logging;
using Otc.Validations.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maverick.Application
{
    public class FilmesService : IFilmesService
    {
        private readonly ITmdbAdapter tmdbAdapter;
        private readonly ApplicationConfiguration configuration;
        private readonly ILogger logger;

        public FilmesService(ITmdbAdapter tmdbAdapter, ApplicationConfiguration 
            configuration, ILoggerFactory loggerFactory)
        {
            this.tmdbAdapter = tmdbAdapter ?? 
                throw new ArgumentNullException(nameof(tmdbAdapter));

            this.configuration = configuration ?? 
                throw new ArgumentNullException(nameof(configuration));

            logger = loggerFactory?.CreateLogger<FilmesService>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        private readonly IEnumerable<string> termosNaoPermitidos = new string[]
        {
            "pornografia"
        };

        public async Task<IEnumerable<Filme>> ObterFilmesAsync(
            Pesquisa pesquisa)
        {
            if (pesquisa == null)
            {
                throw new ArgumentNullException(nameof(pesquisa));
            }

            ValidationHelper.ThrowValidationExceptionIfNotValid(pesquisa);

            // Aplica regra sobre o termo de pesquisa
            if(termosNaoPermitidos.Any(x => pesquisa.TermoPesquisa.Contains(x)))
            {
                throw new BuscarFilmesCoreException(
                    BuscarFilmesCoreError.TermoDePesquisaNaoPermitido);
            }

            logger.LogInformation("Realizando chamada ao TMDb com os seguintes " +
                "criterios de pesquisa: {@CriteriosPesquisa}", 
                new { Criterios = pesquisa, configuration.Idioma });

            IEnumerable<Filme> resultado = await tmdbAdapter
                .GetFilmesAsync(pesquisa, configuration.Idioma);

            logger.LogInformation("Chamada ao TMDb concluida com sucesso.");

            return resultado;
        }
    }
}
