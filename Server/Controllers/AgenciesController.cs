using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Shared.ViewModels;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgenciesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork _UnitOfWork;

        private readonly FunctionalUnitOfWork functionalUnitOfWork;

        public AgenciesController()
        {
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shared.Functional.Agency>>> List()
        {
            var agencies = new List<Shared.Functional.Agency>
            {
                new Shared.Functional.Agency
                {

                    AgencyName = "Institut Abir Hourany", 
                    DisplayName = "Institut Abir Hourany",
                    Address = "Cardinal Sayegh street",
                    Country = "Lebanon",
                    City = "Jisr el bacha, Beiruth",
                    Email = "abeer@abeer.io",
                    PhoneNumber = "+961 1 494 970",
                    PhotoUrl = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBwgHBgkIBwgKCgkLDRYPDQwMDRsUFRAWIB0iIiAdHx8kKDQsJCYxJx8fLT0tMTU3Ojo6Iys/RD84QzQ5OjcBCgoKDQwNGg8PGjclHyU3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3N//AABEIAKAAoAMBIgACEQEDEQH/xAAcAAAABwEBAAAAAAAAAAAAAAABAgMEBQYHAAj/xAA9EAACAQMCAwUFBwIEBwEAAAABAgMABBEFIQYSMRNBUWFxFCIygZEHI0KxwdHwUqEVYoLhJENTY3LC8UT/xAAZAQADAQEBAAAAAAAAAAAAAAAAAwQCAQX/xAAiEQADAAMAAgEFAQAAAAAAAAAAAQIDESESMQQTIjJBUWH/2gAMAwEAAhEDEQA/ALeBtQgV2KECljjgKOKLijqK6AGN6NXUYb0AIzTRxcokkVSxwoJxk+VNbiZTCXXmJBBO+QR1+H5Dv+VG1G3jn7LnYDlkGG8D/P0otwezEgZ+wCDlcAEnPTYj0H0O9KddOpCGqas2nw2jKO3a5dQLZM84JHNzEY226noPWkrTXVlkESxtJIFZiRsDjA7upyenfiovirUnsrNLayiuG1G5do0giXDHnJBfIGw3UepG9E0qE3enaZqKllnwvarIuDA4IjflHXvyO7r3bV3obLNY6tZXgUxTDJYrhsg5Bxj126U/NUTi0waboDYRi9vJGLZUkIXmxgsT3nqcbelSvB3Ei6tDHbXJHtITIP8AUP3FdVf07rm0WU1wFDXVsyFrqGuoAA0UijmgoALii4o9BigBOhAocUIrmwOAowrhQgUbACjBa7FGWjYDLVmKWMnKwTmHvsT3Dpj+/wDaqPqWvx2kEaxvn2f3Q8ifEMbZwRnv8fGpnj/VXsrFRGjshPvco+I9wqsadw+2pFLjUpWI6iBdlHr41Pb70oxRtDReOtUSG57FI1WZBGJXLdoDsSVORjO2x22FLjjzUUhCXVrbzzN7xuGLhj02Zehxj8jU5ecOW8kH3USgqMAY6ioC+0RGjX3d12Kse7yoWRGng0RHEHEd5rNrHBc2qxtHKXLqwPMMbDoPr30jo+py6fqunS22XYy4UeIz7w+honENqsJVeZyqjYgbmmFhzSPClsSZlJ9/OOuw9KatNbEvnD0Ojq6K6/CwyPShpDTzzWMGSCRGAceNL1vfBfoCuzXGuroHV1dQUAdQGhrqAC4ocV2KNisgBQihFdXAOoQN64CjCgDO+PpGnlulH/54l5B3ZJz+g+tBpPEGn88MDmVHYDBZMD60544tys96fwSwoSP9X7Cq1ofDck0MVysyjtACzNksuMjA3xj1pD8XvZXjdJLRY+Kda1DT4itnEkaYBM7DmJz3KvearFnqFzdagLWS0u/aD8bTE83nle4VeHt4LiOKG5PMhQD3j+1OrTTrHTo5JIYkXPUjct6k1malLqGVFN72ULiew5raYLhXhCnI9aiLbSLiG5WZVXEmBgHox6+oq2agwkv5lkA7OdeU57s/74pqZVEltCImR4055HbGx5iuBj0z8xXVbS0YqJ30vPCs5awWCVlMqd3fipsVnceqPY6hb38WWt8NFMo9Rg/KtBjcSxrIvwsMim462ifJOmC1BRsbUFMFgUFCaCugdXV1Aa6AbFDXAUOKyAFGxXChrgAYoyjcVwpO7doraR48c4G2eg86AKNxp7upSwk7yxjl3zyr+In+d9NNAme2jFvIOXIEkfmp6U51izR7ztLqU4kcpk7lgPiY+WxwKQnL+x3WpLGFQIqwr/So6VM+7LJetDp76DtkFxNzcuyqi9PXHWlzercxYj5gmfxDFQuk6lp2oZLTGJ/+ZEG5TSmo6zaQkQWWWPQBdzWGihNMT1qRIrcycwDqcj9qgo7mYh5Hi5C+EUk5wcHb+9StvZT3syy3my5yE/eo6/zy3Ma/FG+R5HORWo/grJ/RzoMytBcx3JPYuxfPXkNafoUyTaTbckiuUTlYqe8HrWLc8kUErQgspkwMnbffAFX3gW8aF0ikJ5JwQvgGFMX20IpbkvgFFYUOcjIopbNOJwCKCjUFdALXUNBQAZOtKY3oqjaj1wAMVwoaBmVQWZgqjqT0FAAiuO+VG5x0qKutX6rZqH/7h6fId9Qk2uajp1wrxyLMjjEkT75+fd8qW8s70OnDbWw2vcMajqV6WsnhhgCYXtCdsnJ2xVc4otdT0gQWmqXEhtJthNGMRlv6T4dNq1rQ7+y4gsDLACkinllif4lb9vOm+taOmqaZPpt/Gs9rKuMN8Q8CD4itrHLWzP1KT0ZKukWE9r2YjUEeNOtMsbS2Vl7JRJ3N40lb211pV1JpeoHM8G8cpGO2j7j6+NSKSxN1FSVuXo9CfGltC1oRzYx0qnassqatc9iSpL1a5CFk50bBHhTPUNN9sDTFSY+rsnVT03omtM5a2tFJguJ4rsxzJgBskEYGat+hXLoHeUt2rOGXwyB/PrUAqSWV2gvk7aHIBbvA8atCWxsLvkA7W3kj5ie/G2GHljrW6oQp17LppWrRXtuhB5X5RkGpHmBxg1ntw72l0PZnwYgNh3gnP61ctPuDNbRuSPeXNNi98E5I11EjmhzSYORQg0wUGrqGgrpwWAoaDNdmuAc7ciMx6AZqupc3GoKGuCBH3IBgH1qa1Ikadckdeyb8qrVtcPHbNyqGIAxvjApGZv0U4JXtkgUVR0+tQupxNPIMBcCmV7xDdQICkKfeAlC3ePH86r1yuqarLzu7+WDygUlSVeRatAv7nSNcjdcsre66Z+Jc/mOtai8uC2Omds1lfAWiS3E8sNxc/wDFW7CRFclgyHvHmD+YrSSrgY7xVeHfiQ52vMrvHuhvrWll9OhWLVbX7y3I6yY6p8+g/wB6zjRtWi1JCuOzuUH3kBGCCOv87q3GJ2Rcke90zmsd+1/hY6fejifSA0cUsg9qCHHZSbAOMdzd/n61q8SszizOH/gtzr+LKmnFtJNEr+zykB15XAOzDwIqnaRxTFOBb6sOzk6LOowD/wCQ7j51YoJHt5Y35hJA/Rgcioqx1Hs9GMk2uA3FsGJyoPligubsxafGQeWW1DAf5l649OoqVuYQfh3FQGsEW0Ek8gDIinnU/iXwrM9ejtzwlYoVu27SLlGIl2zsQdx9M4+VTuhS4tEif4o8r671U9EvbC6igu7GUxtCoR4mb8PmPrvUrpF7JHqMtrIoVzh1HQY/m1MW5olr7pLqh2o1JQMCox0pWq09krD91BQ91BQcDE0KGidaMnWgBvrJ5dKujnH3RqsWzhoXGDnG9WXXgTo91j+n9RVUs2yjDxFT5vaK/jr7WRVqEn1BlnBZo0CqD3DqPzqa5UjUkDFRMQEesH/NH+tSlwcIaTRTPEM7DUn07iSC6iORH8aj8SHYj6H64rVJSriOSEhlkUMpHQg1kmn6W+pW+r38TlZLNAYl7nOCzKfkB9asP2R8Zw60r6Rekpdw5ks+Y/HEeo9V3+R8qrwKkiL5NS3z2WDUtVmwUtB2S5xzMPePoO4VJ2LWmt6TPbXSCaNozDcRkYDAjBpvr9gUufaVT3HPv9wB8SfDyFLcPgLcsjodgQkvNhSM7gD1+dIi8k5tUzFKXj2jztx5wvPwnrsli/M1tIO0tZSN3jztn/MOh+tRem6td6aOWCTMRO8T7of2+VelvtH4Rt+LOH2tyVjvoMyWcp/C/wDSfI9Poe6vLtzDNa3EtvcxtFPExSSNhurDYg16Gk/YiaafDRdD4x02a15NRka3lQfiGQw8iO+qzxTxENWYwWaMloDkltjJ4bdwqug0Y9KVOCJryH18i6nTFrK7lspQ8Z9a0PSrz2vR5LiQKLiBR2TjYFeYbf3/ACrM2O9XfhqZW4RljyC4uWhXJwTzcp/IEVnPPNnMFd0azay+6pPhTsNUNBciRUcbBgCB5U/ikLVmXw7S0x6DmhpNDmlK2YCgYoy9aA0ANBwJqi9ppd0vjE35VSdOfmBFX4p2kToejKR9RWdaeSjsrbEbUjMv2V/Gf6C3qlJUlX4lbIp5PMHtBJnYrTS+cBd6b6dHJfyW+nR5DTScpPgvUn5DNKU+Q9147LxwRZCHh5GkUH2pmlYeIbYf2ArEtWjvOFeKpRas0FxY3LNbSD+jJKn0xt9RXouGNIokijUBEUKoHcB0rDPtfiKcaSn+u2iYf3H6VbHOHm09ts3jhHXbHjPh2O/RVAk9y4gzvFIOo/UeRFclhJ2xWfEcUJ+6KHqObNef/s04vfhTXQJ3f/DbvEd0o/BvtIPNfyJ8q9EXdwy6bJdQx9thS0aq3x+G/hRkxTT3Rmaa4hF9TkfUfZbhVQHeJl5sEeZxjJ8KyD7a+Hl9oXiC1UczgJeKBjvwj+vRT/pq2G4udQtGv7265d+WBY9irg7gA+XfUvfm0vpVhuOW5s5bchgwGJc7EfOs4snk2jVx4nmgGj5qS4r0STh3XLjTmLPGuHgkYfHGd1P6eoNRPNtVIsFzVz4IjhvNN1HTpmCl+WRGP4D0DfI8v1qksc1ZuEZngv47tVDIAVkU9HUjBX6UrN+I3B+ZqlmTywuylduR1O3K2M/ntUtbmoSylE0EcSSdp7ysG8VG4P0AFS0Lb1Nj9Dcnsk4z0pamsTbU4U5pwoDNcOtB3mjUHBdD4Vn+qwmx1e5VgQGcunmG3/cfKr6lNdU0iz1QL7Ujc6jAdDgjy9Kzc+SG478HszS8nywAyxY4CruT6CrtwZoMunh7+/Xlu5l5UjP/ACk8D5nbPoKk9M0HTdNk7S2tx23/AFZDzNjyJ6fKpQGszGjuTK69CqisN+2MSLxm3aOGU2kXIAN1XLbH55PzFbkm5A8a89/aLqsOscX31xb7wx4gVifi5MgkeWc0+fYllWYBs7Gtk+xvjBri3Xh3UHJmtlLWTFvij6snqvUeWfCsfNHs7q40+9gvLSQx3EEgkjcdzCmNbFnoHVbCWBpLhka5lMhIYLlYkJPRem3nUTdXEkM6RJcmWMSFu1PTGOg/mKsejatZcT8M2ep26DDqVuYs55JPxp+3lioDXrCKyRlkIYSA9kuSdh+Jj/6+NQZYeOvJeijHSpaYw4/0A8TcMQ39pGDfWIJTxlTGWT1zuPPbvrEWwBtmt64c1AGCSG6choDzb+P8zWb8baAF1y2ubZCsGpThWAHwSE4P16j51bF7naEVOmO7PgmGPhuz1O4ZnuLhecoeiA/Dt6b/AEpSzs4bSERxjp+dXniMrBpMcKn3U5VHoBiqZzbHepMltsv+PKSLDwkCwuA+4yCM93pVkVRmoDhRP+EllA6tyj5f/ang2TXY4hWR9HUTYp0jUxQ0ujUxMUO1oxFANqEmtGQQaNzUnQiuAKhqOtJJSyCgCL4t1hdC4du74n7wJyQjxc7D9/ka84El2LMST51e/tU4mGramunWjg2loxBK9JJO8+g6D51RBtTZXDLYPdSbilDRGrZkuH2X8Vjh7Wza3jD/AA2+9yXmziJ8ELJj54PkfKtp1Sz57Rocx+0onMjlQ3JnvFeYX761Tgvja7fh/wBluYVlms2WNLhiffTGAreY/QUvJrx6ahN1pC+nWF5/i7WtpGZJ3B7SLPQbdT07xU3qtpDZQxjU41E8LLKkbb4ZcEY8ajjxGgleW2jhtZ5Rys8Wedh4Z/YUzmmeb72d3kY98hJP1NSTkULSLHgdvotxheMz2tvGcgoXI7yNsCoZ7OaOyN40TdiMDm6jJzgeuxpa4kjMnavu+MZO5x4Utw5Cbu/kmuO09gXYKp91pwMrzDwADb+OPGucpm9fTn2WTR7c2WnQwsMOBlvU7mn6mkAc0qhpqJ2OFpZTSKGlRXUYP//Z",
                    MapUrl="https://www.google.fr/maps/place/Institut+Abir+Hourany/@33.868981,35.538604,15z/data=!4m5!3m4!1s0x0:0xbbc1b861e22b95d!8m2!3d33.868981!4d35.538604"
                }
            };

            return Ok(agencies);
        }
    }
}
