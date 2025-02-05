﻿using AutoMapper;
using sReportsV2.Common.Constants;
using sReportsV2.Common.CustomAttributes;
using sReportsV2.Common.Extensions;
using sReportsV2.Domain.Extensions;
using sReportsV2.DTOs.Umls;
using sReportsV2.DTOs.Umls.DatOut;
using System;
using System.Collections.Generic;
using UMLSClient.Client;
using UMLSClient.UMLSClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using sReportsV2.BusinessLayer.Interfaces;

namespace sReportsV2.Controllers
{
    public class UmlsController : BaseController
    {
        private readonly IMapper Mapper;
        public UmlsController(IMapper mapper, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider, IConfiguration configuration, IAsyncRunner asyncRunner) : base(httpContextAccessor, serviceProvider, configuration, asyncRunner)
        {
            Mapper = mapper;
        }

        [SReportsAuthorize(Permission = PermissionNames.UMLS, Module = ModuleNames.Thesaurus)]
        public ActionResult Search(UmlsDataIn dataIn)
        {
            dataIn = Ensure.IsNotNull(dataIn, nameof(dataIn));

            Client umlsClient = new Client();
            var result = Mapper.Map<UmlsDataOut<SearchResultDataOut>>(umlsClient.GetSearchResult(dataIn.SearchTerm, dataIn.PageSize, dataIn.Page));
            return PartialView("ConceptsRows",result);
        }

        [SReportsAuthorize]
        public ActionResult GetDefinitions(string id)
        {
            Client client = new Client();
            var result = Mapper.Map<UmlsDataOut<List<ConceptDefinitionDataOut>>>(client.GetConceptDefinition(id));
            if (result == null)
            {
                result = new UmlsDataOut<List<ConceptDefinitionDataOut>>()
                {
                    Result = new List<ConceptDefinitionDataOut>()
                };
            }

            return PartialView("DefinitionsList",result);
        }

        [SReportsAuthorize]
        public ActionResult GetAtoms(string id)
        {
            Client client = new Client();
            var result = Mapper.Map<UmlsDataOut<List<AtomDataOut>>>(client.GetAtomsResult(id));
            if(result == null)
            {
                result = new UmlsDataOut<List<AtomDataOut>>()
                {
                    Result = new List<AtomDataOut>()
                };
            }
            return PartialView("AtomsList",result);
        }
    }
}