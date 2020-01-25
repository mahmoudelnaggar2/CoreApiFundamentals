using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper,
            LinkGenerator linkGenerator)
        {
            this._repository = repository;
            this._mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var results = await _repository.GetTalksByMonikerAsync(moniker, true);

                return _mapper.Map<TalkModel[]>(results);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "faild to get talks from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var result = await _repository.GetTalkByMonikerAsync(moniker, id);

                return _mapper.Map<TalkModel>(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "faild to get talks from the database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Talk>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);

                if (camp == null)
                    return BadRequest("Camp not found");

                var talk = _mapper.Map<Talk>(model);

                talk.Camp = camp;

                if (model.Speaker == null)
                    return BadRequest("Speaker ID is required");

                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);

                if (speaker == null)
                    BadRequest("Speaker could't be found");

                talk.Speaker = speaker;

                _repository.Add(talk);

                if (await _repository.SaveChangesAsync())
                {
                    var location = linkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId});

                    return Created(location, _mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "faild to get talks from the database");
            }

            return BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound("Talk not found");

                _mapper.Map(model, talk);

                if (model.Speaker != null)
                {
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);

                    if (speaker != null)
                        talk.Speaker = speaker;
                }

                if (await _repository.SaveChangesAsync())
                    return _mapper.Map<TalkModel>(talk);
               
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "faild to get talks from the database");
            }

            return BadRequest();
        }

        [HttpDelete("id:int")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound("The Talk Not found");

                _repository.Delete(talk);

                if (await _repository.SaveChangesAsync())
                    return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "faild to get talks from the database");
            }

            return BadRequest();
        }
    }
}
